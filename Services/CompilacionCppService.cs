namespace EndForge.Services;

public sealed class CompilacionCppService {
    private const int LimiteSalidaCompilacion = 1024 * 1024;
    private const int LimiteSalidaLocalizador = 32 * 1024;
    private static readonly TimeSpan TiempoMaximoCompilacion = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan TiempoMaximoLocalizador = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan TiempoMaximoConsultaArtefacto = TimeSpan.FromSeconds(30);

    private readonly SeleccionSolucionesService seleccionSolucionesService;

    public CompilacionCppService()
        : this(new SeleccionSolucionesService()) {
    }

    public CompilacionCppService(
        SeleccionSolucionesService seleccionSolucionesService) {
        this.seleccionSolucionesService = seleccionSolucionesService;
    }

    public async Task<ResultadoCompilacionCpp> CompilarAsync(
        SolicitudCompilacionCpp solicitud,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(solicitud);

        if (!EsConfiguracionAdmitida(solicitud.Configuracion) ||
            !EsPlataformaAdmitida(solicitud.Plataforma)) {
            return new ResultadoCompilacionCpp {
                Estado = EstadoCompilacionCpp.ErrorInfraestructura,
                Error = new ArgumentException(
                    "La configuración de compilación solicitada no es válida."
                )
            };
        }

        ResultadoResolucionProyectoEvaluacionCpp resolucion =
            seleccionSolucionesService.ResolverProyectoParaEvaluacion(
                solicitud.RutaPractica
            );

        if (!resolucion.EsExitosa) {
            return new ResultadoCompilacionCpp {
                Estado = EstadoCompilacionCpp.ProyectoNoDisponible,
                ResolucionProyecto = resolucion,
                Error = resolucion.Error
            };
        }

        ConfiguracionSolucionCpp? configuracionSeleccionada;

        try {
            configuracionSeleccionada = SeleccionarConfiguracionSolucion(
                resolucion.RutaSolucion
            );
        } catch (Exception ex) {
            return new ResultadoCompilacionCpp {
                Estado = EstadoCompilacionCpp.ConfiguracionNoDisponible,
                ResolucionProyecto = resolucion,
                Error = ex
            };
        }

        if (configuracionSeleccionada == null) {
            return new ResultadoCompilacionCpp {
                Estado = EstadoCompilacionCpp.ConfiguracionNoDisponible,
                ResolucionProyecto = resolucion
            };
        }

        ResultadoLocalizacionMsBuildCpp localizacion =
            await LocalizarMsBuildAsync(cancellationToken).ConfigureAwait(false);

        if (localizacion.Estado == EstadoProcesoControladoCpp.Cancelada) {
            return new ResultadoCompilacionCpp {
                Estado = EstadoCompilacionCpp.Cancelada,
                ResolucionProyecto = resolucion
            };
        }

        if (string.IsNullOrWhiteSpace(localizacion.RutaMsBuild)) {
            return new ResultadoCompilacionCpp {
                Estado = EstadoCompilacionCpp.MsBuildNoDisponible,
                ResolucionProyecto = resolucion,
                SalidaEstandar = localizacion.SalidaEstandar,
                SalidaError = localizacion.SalidaError,
                Error = localizacion.Error
            };
        }

        string directorioRaizTemporal = "";
        string directorioSesion = "";
        string identificadorPropiedad = "";
        bool sesionTransferida = false;

        try {
            (directorioRaizTemporal, directorioSesion, identificadorPropiedad) =
                DirectorioTemporalEvaluacionCpp.CrearSesion();

            string directorioSalida = Path.Combine(directorioSesion, "bin");
            string directorioIntermedios = Path.Combine(directorioSesion, "obj");
            Directory.CreateDirectory(directorioSalida);
            Directory.CreateDirectory(directorioIntermedios);

            List<string> argumentosComunes = CrearArgumentosComunes(
                configuracionSeleccionada.Configuracion,
                configuracionSeleccionada.Plataforma,
                directorioSalida,
                directorioIntermedios
            );
            List<string> argumentosCompilacion = new() {
                resolucion.RutaSolucion,
                "/nologo",
                "/t:Build"
            };
            argumentosCompilacion.AddRange(argumentosComunes);
            argumentosCompilacion.Add("/p:BuildProjectReferences=true");
            argumentosCompilacion.Add("/p:PreferredToolArchitecture=x64");
            argumentosCompilacion.Add("/m:1");
            argumentosCompilacion.Add("/nr:false");
            argumentosCompilacion.Add("/v:minimal");

            ResultadoProcesoControladoCpp compilacion =
                await ProcesoControladoCpp.EjecutarAsync(
                    new SolicitudProcesoControladoCpp {
                        Archivo = localizacion.RutaMsBuild,
                        DirectorioTrabajo = Path.GetDirectoryName(resolucion.RutaSolucion)!,
                        Argumentos = argumentosCompilacion,
                        TiempoMaximo = TiempoMaximoCompilacion,
                        LimiteCaracteresSalida = LimiteSalidaCompilacion,
                        VariablesEntorno = new Dictionary<string, string?> {
                            ["MSBUILDDISABLENODEREUSE"] = "1"
                        }
                    },
                    cancellationToken
                ).ConfigureAwait(false);

            EstadoCompilacionCpp? estadoProceso = MapearEstadoProceso(compilacion);

            if (estadoProceso != null) {
                return CrearResultadoProceso(
                    estadoProceso.Value,
                    resolucion,
                    compilacion
                );
            }

            if (compilacion.CodigoSalida != 0) {
                EstadoCompilacionCpp estadoFallo;

                if (EsConfiguracionInexistente(
                    compilacion.SalidaEstandar,
                    compilacion.SalidaError)) {
                    estadoFallo = EstadoCompilacionCpp.ConfiguracionNoDisponible;
                } else if (EsFalloDeEntorno(
                    compilacion.SalidaEstandar,
                    compilacion.SalidaError)) {
                    estadoFallo = EstadoCompilacionCpp.EntornoCompilacionNoDisponible;
                } else {
                    estadoFallo = EstadoCompilacionCpp.ErrorCompilacion;
                }

                return CrearResultadoProceso(
                    estadoFallo,
                    resolucion,
                    compilacion
                );
            }

            ResultadoLocalizacionEjecutableCpp ejecutable =
                await LocalizarEjecutableAsync(
                    localizacion.RutaMsBuild,
                    resolucion.RutaProyectoCpp,
                    configuracionSeleccionada.Configuracion,
                    configuracionSeleccionada.Plataforma,
                    directorioSalida,
                    directorioIntermedios,
                    cancellationToken
                ).ConfigureAwait(false);

            if (ejecutable.Estado != EstadoCompilacionCpp.Exitosa) {
                return new ResultadoCompilacionCpp {
                    Estado = ejecutable.Estado,
                    ResolucionProyecto = resolucion,
                    SalidaEstandar = compilacion.SalidaEstandar,
                    SalidaError = CombinarSalidas(
                        compilacion.SalidaError,
                        ejecutable.Detalle
                    ),
                    CodigoSalida = compilacion.CodigoSalida,
                    SalidaTruncada = compilacion.SalidaTruncada,
                    Error = ejecutable.Error
                };
            }

            SesionCompilacionCpp sesion = new(
                Path.GetFullPath(solicitud.RutaPractica),
                resolucion.RutaSolucion,
                resolucion.RutaProyectoCpp,
                ejecutable.RutaEjecutable,
                directorioSesion,
                directorioRaizTemporal,
                identificadorPropiedad
            );
            sesionTransferida = true;

            return new ResultadoCompilacionCpp {
                Estado = EstadoCompilacionCpp.Exitosa,
                ResolucionProyecto = resolucion,
                SalidaEstandar = compilacion.SalidaEstandar,
                SalidaError = compilacion.SalidaError,
                CodigoSalida = compilacion.CodigoSalida,
                SalidaTruncada = compilacion.SalidaTruncada,
                Sesion = sesion
            };
        } catch (OperationCanceledException) {
            return new ResultadoCompilacionCpp {
                Estado = EstadoCompilacionCpp.Cancelada,
                ResolucionProyecto = resolucion
            };
        } catch (Exception ex) {
            return new ResultadoCompilacionCpp {
                Estado = EstadoCompilacionCpp.ErrorInfraestructura,
                ResolucionProyecto = resolucion,
                Error = ex
            };
        } finally {
            if (!sesionTransferida &&
                !string.IsNullOrEmpty(directorioRaizTemporal) &&
                !string.IsNullOrEmpty(directorioSesion) &&
                !string.IsNullOrEmpty(identificadorPropiedad)) {
                DirectorioTemporalEvaluacionCpp.EliminarSesionPropia(
                    directorioRaizTemporal,
                    directorioSesion,
                    identificadorPropiedad
                );
            }
        }
    }

    private static List<string> CrearArgumentosComunes(
        string configuracion,
        string plataforma,
        string directorioSalida,
        string directorioIntermedios) {
        string salida = AsegurarSeparadorFinal(directorioSalida);
        string intermedios = AsegurarSeparadorFinal(directorioIntermedios);

        return new List<string> {
            $"/p:Configuration={configuracion}",
            $"/p:Platform={plataforma}",
            $"/p:OutDir={salida}",
            $"/p:IntDir={intermedios}"
        };
    }

    private static async Task<ResultadoLocalizacionMsBuildCpp> LocalizarMsBuildAsync(
        CancellationToken cancellationToken) {
        string rutaVsWhere = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft Visual Studio",
            "Installer",
            "vswhere.exe"
        );

        if (!File.Exists(rutaVsWhere)) {
            return new ResultadoLocalizacionMsBuildCpp();
        }

        ResultadoLocalizacionMsBuildCpp estable = await EjecutarVsWhereAsync(
            rutaVsWhere,
            incluirVersionPreliminar: false,
            cancellationToken
        ).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(estable.RutaMsBuild) ||
            estable.Estado == EstadoProcesoControladoCpp.Cancelada) {
            return estable;
        }

        return await EjecutarVsWhereAsync(
            rutaVsWhere,
            incluirVersionPreliminar: true,
            cancellationToken
        ).ConfigureAwait(false);
    }

    private static async Task<ResultadoLocalizacionMsBuildCpp> EjecutarVsWhereAsync(
        string rutaVsWhere,
        bool incluirVersionPreliminar,
        CancellationToken cancellationToken) {
        List<string> argumentos = new() {
            "-latest",
            "-products",
            "*",
            "-requires",
            "Microsoft.VisualStudio.Component.VC.Tools.x86.x64",
            "-find",
            @"MSBuild\**\Bin\MSBuild.exe"
        };

        if (incluirVersionPreliminar) {
            argumentos.Add("-prerelease");
        }

        ResultadoProcesoControladoCpp resultado =
            await ProcesoControladoCpp.EjecutarAsync(
                new SolicitudProcesoControladoCpp {
                    Archivo = rutaVsWhere,
                    DirectorioTrabajo = Path.GetDirectoryName(rutaVsWhere)!,
                    Argumentos = argumentos,
                    TiempoMaximo = TiempoMaximoLocalizador,
                    LimiteCaracteresSalida = LimiteSalidaLocalizador
                },
                cancellationToken
            ).ConfigureAwait(false);

        string rutaMsBuild = resultado.Estado == EstadoProcesoControladoCpp.Exitosa &&
            resultado.CodigoSalida == 0
                ? resultado.SalidaEstandar
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(linea => linea.Trim().Trim('"'))
                    .Where(ruta => Path
                        .GetFileName(ruta)
                        .Equals("MSBuild.exe", StringComparison.OrdinalIgnoreCase))
                    .Where(Path.IsPathRooted)
                    .Where(File.Exists)
                    .OrderBy(ruta => ruta, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(ruta => ruta, StringComparer.Ordinal)
                    .FirstOrDefault() ?? ""
                : "";

        return new ResultadoLocalizacionMsBuildCpp {
            Estado = resultado.Estado,
            RutaMsBuild = rutaMsBuild,
            SalidaEstandar = resultado.SalidaEstandar,
            SalidaError = resultado.SalidaError,
            Error = resultado.Error
        };
    }

    private static async Task<ResultadoLocalizacionEjecutableCpp> LocalizarEjecutableAsync(
        string rutaMsBuild,
        string rutaProyectoCpp,
        string configuracion,
        string plataforma,
        string directorioSalida,
        string directorioIntermedios,
        CancellationToken cancellationToken) {
        List<string> argumentos = new() {
            rutaProyectoCpp,
            "/nologo",
            "-getProperty:TargetPath"
        };
        argumentos.AddRange(CrearArgumentosComunes(
            configuracion,
            plataforma,
            directorioSalida,
            directorioIntermedios
        ));
        argumentos.Add("/nr:false");
        argumentos.Add("/v:quiet");

        ResultadoProcesoControladoCpp consulta =
            await ProcesoControladoCpp.EjecutarAsync(
                new SolicitudProcesoControladoCpp {
                    Archivo = rutaMsBuild,
                    DirectorioTrabajo = Path.GetDirectoryName(rutaProyectoCpp)!,
                    Argumentos = argumentos,
                    TiempoMaximo = TiempoMaximoConsultaArtefacto,
                    LimiteCaracteresSalida = 64 * 1024,
                    VariablesEntorno = new Dictionary<string, string?> {
                        ["MSBUILDDISABLENODEREUSE"] = "1"
                    }
                },
                cancellationToken
            ).ConfigureAwait(false);

        if (consulta.Estado == EstadoProcesoControladoCpp.Cancelada) {
            return new ResultadoLocalizacionEjecutableCpp {
                Estado = EstadoCompilacionCpp.Cancelada
            };
        }

        string[] rutasDeclaradas = consulta.Estado == EstadoProcesoControladoCpp.Exitosa &&
            consulta.CodigoSalida == 0
                ? ExtraerRutasEjecutables(
                    consulta.SalidaEstandar,
                    Path.GetDirectoryName(rutaProyectoCpp)!
                )
                : Array.Empty<string>();

        if (rutasDeclaradas.Length == 1) {
            string declarada = rutasDeclaradas[0];

            if (!DirectorioTemporalEvaluacionCpp.EstaDentroDe(directorioSalida, declarada)) {
                return new ResultadoLocalizacionEjecutableCpp {
                    Estado = EstadoCompilacionCpp.EjecutableFueraDeDirectorioTemporal,
                    Detalle = consulta.SalidaError
                };
            }

            if (File.Exists(declarada) &&
                DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                    directorioSalida,
                    declarada)) {
                return new ResultadoLocalizacionEjecutableCpp {
                    Estado = EstadoCompilacionCpp.Exitosa,
                    RutaEjecutable = declarada
                };
            }
        } else if (rutasDeclaradas.Length > 1) {
            return new ResultadoLocalizacionEjecutableCpp {
                Estado = EstadoCompilacionCpp.EjecutableAmbiguo,
                Detalle = consulta.SalidaError
            };
        }

        string[] ejecutablesGenerados = DirectorioTemporalEvaluacionCpp
            .EnumerarArchivosSinPuntosDeReanalisis(directorioSalida)
            .Where(ruta => Path
                .GetExtension(ruta)
                .Equals(".exe", StringComparison.OrdinalIgnoreCase))
            .OrderBy(ruta => ruta, StringComparer.OrdinalIgnoreCase)
            .ThenBy(ruta => ruta, StringComparer.Ordinal)
            .ToArray();

        return ejecutablesGenerados.Length switch {
            1 => new ResultadoLocalizacionEjecutableCpp {
                Estado = EstadoCompilacionCpp.Exitosa,
                RutaEjecutable = Path.GetFullPath(ejecutablesGenerados[0])
            },
            > 1 => new ResultadoLocalizacionEjecutableCpp {
                Estado = EstadoCompilacionCpp.EjecutableAmbiguo,
                Detalle = consulta.SalidaError
            },
            _ => new ResultadoLocalizacionEjecutableCpp {
                Estado = EstadoCompilacionCpp.EjecutableNoGenerado,
                Detalle = consulta.SalidaError,
                Error = consulta.Error
            }
        };
    }

    private static string[] ExtraerRutasEjecutables(
        string salida,
        string directorioProyecto) {
        List<string> rutas = new();

        foreach (string linea in salida.Split(
            new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries)) {
            string candidata = linea.Trim().Trim('"');

            if (!Path.GetExtension(candidata).Equals(
                ".exe",
                StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            try {
                rutas.Add(Path.IsPathRooted(candidata)
                    ? Path.GetFullPath(candidata)
                    : Path.GetFullPath(candidata, directorioProyecto));
            } catch (Exception ex) when (
                ex is ArgumentException or NotSupportedException or PathTooLongException) {
                // Se ignoran líneas que no representan una ruta válida.
            }
        }

        return rutas
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static EstadoCompilacionCpp? MapearEstadoProceso(
        ResultadoProcesoControladoCpp proceso) {
        return proceso.Estado switch {
            EstadoProcesoControladoCpp.Exitosa => null,
            EstadoProcesoControladoCpp.Cancelada => EstadoCompilacionCpp.Cancelada,
            EstadoProcesoControladoCpp.TiempoExcedido => EstadoCompilacionCpp.TiempoExcedido,
            EstadoProcesoControladoCpp.SalidaExcesiva => EstadoCompilacionCpp.SalidaExcesiva,
            EstadoProcesoControladoCpp.ErrorInicio => EstadoCompilacionCpp.ErrorInicio,
            EstadoProcesoControladoCpp.ErrorEjecucion => EstadoCompilacionCpp.ErrorInfraestructura,
            _ => EstadoCompilacionCpp.ErrorInfraestructura
        };
    }

    private static ResultadoCompilacionCpp CrearResultadoProceso(
        EstadoCompilacionCpp estado,
        ResultadoResolucionProyectoEvaluacionCpp resolucion,
        ResultadoProcesoControladoCpp proceso) {
        return new ResultadoCompilacionCpp {
            Estado = estado,
            ResolucionProyecto = resolucion,
            SalidaEstandar = proceso.SalidaEstandar,
            SalidaError = proceso.SalidaError,
            CodigoSalida = proceso.CodigoSalida,
            SalidaTruncada = proceso.SalidaTruncada,
            Error = proceso.Error
        };
    }

    private static bool EsFalloDeEntorno(string salida, string error) {
        string diagnostico = salida + Environment.NewLine + error;

        return diagnostico.Contains("MSB8020", StringComparison.OrdinalIgnoreCase) ||
            diagnostico.Contains("MSB8036", StringComparison.OrdinalIgnoreCase) ||
            diagnostico.Contains("MSB4019", StringComparison.OrdinalIgnoreCase) ||
            diagnostico.Contains("MSB4276", StringComparison.OrdinalIgnoreCase);
    }

    private static bool EsConfiguracionInexistente(string salida, string error) {
        string diagnostico = salida + Environment.NewLine + error;

        return diagnostico.Contains("MSB4126", StringComparison.OrdinalIgnoreCase) ||
            diagnostico.Contains("MSB8013", StringComparison.OrdinalIgnoreCase);
    }

    private static ConfiguracionSolucionCpp? SeleccionarConfiguracionSolucion(
        string rutaSolucion) {
        List<ConfiguracionSolucionCpp> configuraciones = new();
        bool dentroSeccion = false;

        foreach (string linea in File.ReadLines(rutaSolucion)) {
            string contenido = linea.Trim();

            if (!dentroSeccion) {
                dentroSeccion = contenido.StartsWith(
                    "GlobalSection(SolutionConfigurationPlatforms)",
                    StringComparison.Ordinal
                );
                continue;
            }

            if (contenido.StartsWith("EndGlobalSection", StringComparison.Ordinal)) {
                break;
            }

            int separadorAsignacion = contenido.IndexOf('=');

            if (separadorAsignacion <= 0) {
                continue;
            }

            string identificador = contenido[..separadorAsignacion].Trim();
            int separadorPlataforma = identificador.LastIndexOf('|');

            if (separadorPlataforma <= 0 ||
                separadorPlataforma == identificador.Length - 1) {
                continue;
            }

            string configuracion = identificador[..separadorPlataforma].Trim();
            string plataforma = identificador[(separadorPlataforma + 1)..].Trim();

            if (!EsNombreMsBuildSeguro(configuracion) ||
                !EsNombreMsBuildSeguro(plataforma)) {
                continue;
            }

            configuraciones.Add(new ConfiguracionSolucionCpp(
                configuracion,
                plataforma
            ));
        }

        ConfiguracionSolucionCpp[] unicas = configuraciones
            .GroupBy(
                item => $"{item.Configuracion}\0{item.Plataforma}",
                StringComparer.OrdinalIgnoreCase
            )
            .Select(grupo => grupo.First())
            .OrderBy(item => item.Configuracion, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Configuracion, StringComparer.Ordinal)
            .ThenBy(item => item.Plataforma, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Plataforma, StringComparer.Ordinal)
            .ToArray();

        ConfiguracionSolucionCpp[] debug = unicas
            .Where(item => item.Configuracion.Equals(
                "Debug",
                StringComparison.OrdinalIgnoreCase
            ))
            .ToArray();

        if (debug.Length > 0) {
            return BuscarPlataforma(debug, "x64") ??
                BuscarPlataforma(debug, "Win32") ??
                BuscarPlataforma(debug, "x86") ??
                debug[0];
        }

        ConfiguracionSolucionCpp[] release = unicas
            .Where(item => item.Configuracion.Equals(
                "Release",
                StringComparison.OrdinalIgnoreCase
            ))
            .ToArray();

        if (release.Length > 0) {
            return BuscarPlataforma(release, "x64") ??
                BuscarPlataforma(release, "Win32") ??
                BuscarPlataforma(release, "x86") ??
                release[0];
        }

        return unicas.FirstOrDefault();
    }

    private static ConfiguracionSolucionCpp? BuscarPlataforma(
        IEnumerable<ConfiguracionSolucionCpp> configuraciones,
        string plataforma) {
        return configuraciones.FirstOrDefault(item => item.Plataforma.Equals(
            plataforma,
            StringComparison.OrdinalIgnoreCase
        ));
    }

    private static bool EsNombreMsBuildSeguro(string valor) {
        if (string.IsNullOrWhiteSpace(valor) || valor.Length > 128) {
            return false;
        }

        return valor.All(caracter =>
            !char.IsControl(caracter) &&
            caracter is not ';' and not '=' and not '/' and not '\\' and not '"');
    }

    private static bool EsConfiguracionAdmitida(string configuracion) {
        return configuracion.Equals("Debug", StringComparison.Ordinal) ||
            configuracion.Equals("Release", StringComparison.Ordinal);
    }

    private static bool EsPlataformaAdmitida(string plataforma) {
        return plataforma.Equals("x64", StringComparison.Ordinal) ||
            plataforma.Equals("Win32", StringComparison.Ordinal);
    }

    private static string AsegurarSeparadorFinal(string ruta) {
        return Path.EndsInDirectorySeparator(ruta)
            ? ruta
            : ruta + Path.DirectorySeparatorChar;
    }

    private static string CombinarSalidas(string primera, string segunda) {
        if (string.IsNullOrWhiteSpace(primera)) {
            return segunda;
        }

        if (string.IsNullOrWhiteSpace(segunda)) {
            return primera;
        }

        return primera + Environment.NewLine + segunda;
    }

    private sealed class ResultadoLocalizacionMsBuildCpp {
        public EstadoProcesoControladoCpp Estado { get; init; }

        public string RutaMsBuild { get; init; } = "";

        public string SalidaEstandar { get; init; } = "";

        public string SalidaError { get; init; } = "";

        public Exception? Error { get; init; }
    }

    private sealed record ConfiguracionSolucionCpp(
        string Configuracion,
        string Plataforma);

    private sealed class ResultadoLocalizacionEjecutableCpp {
        public EstadoCompilacionCpp Estado { get; init; }

        public string RutaEjecutable { get; init; } = "";

        public string Detalle { get; init; } = "";

        public Exception? Error { get; init; }
    }
}
