using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EndForge.Services;

public sealed class SeleccionSolucionesService {
    public const string MarcadorPlantilla = "00_Plantilla";
    private const string NombreArchivoSeleccion = ".endforge-solution";
    private const int CaracteresMaximosSeleccion = 4096;
    private const long BytesMaximosSolucion = 2 * 1024 * 1024;

    public string[] ObtenerSolucionesOrdenadas(string rutaCarpeta) {
        return Directory
            .EnumerateFiles(rutaCarpeta, "*", SearchOption.TopDirectoryOnly)
            .Where(archivo => Path.GetExtension(archivo).Equals(".sln", StringComparison.OrdinalIgnoreCase))
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(Path.GetFileName, StringComparer.Ordinal)
            .ToArray();
    }

    public string ObtenerRutaRelativa(string rutaRaiz, string rutaSolucion) {
        return Path.GetRelativePath(Path.GetFullPath(rutaRaiz), Path.GetFullPath(rutaSolucion));
    }

    public string TransformarRutaRelativa(string rutaRelativa, string nombreProyecto) {
        if (Path.IsPathRooted(rutaRelativa)) {
            throw new ArgumentException("La ruta de la solución debe ser relativa.", nameof(rutaRelativa));
        }

        return rutaRelativa.Replace(MarcadorPlantilla, nombreProyecto, StringComparison.Ordinal);
    }

    public void GuardarSolucionSeleccionada(string rutaCarpeta, string rutaRelativaSolucion) {
        if (!Path.GetExtension(rutaRelativaSolucion).Equals(".sln", StringComparison.OrdinalIgnoreCase) ||
            !IntentarResolverRutaRelativa(rutaCarpeta, rutaRelativaSolucion, out string rutaSolucion) ||
            !File.Exists(rutaSolucion)) {
            throw new FileNotFoundException("No se encontró la solución esperada de la práctica.");
        }

        File.WriteAllText(
            Path.Combine(rutaCarpeta, NombreArchivoSeleccion),
            rutaRelativaSolucion
        );
    }

    public string? LeerSolucionSeleccionada(string rutaCarpeta) {
        string rutaSeleccion = Path.Combine(rutaCarpeta, NombreArchivoSeleccion);

        try {
            return File.ReadAllText(rutaSeleccion);
        } catch (FileNotFoundException) {
            return null;
        } catch (DirectoryNotFoundException) {
            return null;
        }
    }   

    public ResultadoResolucionProyectoEvaluacionCpp ResolverProyectoParaEvaluacion(
        string rutaPractica) {
        if (!Directory.Exists(rutaPractica)) {
            return CrearResultadoResolucion(
                EstadoResolucionProyectoEvaluacionCpp.CarpetaInexistente
            );
        }

        string rutaPracticaNormalizada;

        try {
            rutaPracticaNormalizada = Path.GetFullPath(rutaPractica);
        } catch (Exception ex) when (
            ex is ArgumentException or NotSupportedException or PathTooLongException) {
            return CrearResultadoResolucion(
                EstadoResolucionProyectoEvaluacionCpp.CarpetaInexistente,
                error: ex
            );
        }

        try {
            ResultadoSeleccionEvaluacionCpp seleccion =
                ResolverSolucionParaEvaluacion(rutaPracticaNormalizada);

            if (seleccion.Estado != EstadoResolucionProyectoEvaluacionCpp.Exitosa) {
                return CrearResultadoResolucion(
                    seleccion.Estado,
                    usaSeleccionGuardada: seleccion.UsaSeleccionGuardada,
                    error: seleccion.Error
                );
            }

            ResultadoProyectoReferenciadoCpp proyecto = ResolverProyectoCppReferenciado(
                rutaPracticaNormalizada,
                seleccion.RutaSolucion
            );

            return CrearResultadoResolucion(
                proyecto.Estado,
                seleccion.RutaSolucion,
                proyecto.RutaProyectoCpp,
                seleccion.UsaSeleccionGuardada,
                proyecto.Error
            );
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoResolucion(
                EstadoResolucionProyectoEvaluacionCpp.ErrorLectura,
                error: ex
            );
        } catch (IOException ex) {
            return CrearResultadoResolucion(
                EstadoResolucionProyectoEvaluacionCpp.ErrorLectura,
                error: ex
            );
        } catch (Exception ex) {
            return CrearResultadoResolucion(
                EstadoResolucionProyectoEvaluacionCpp.ErrorLectura,
                error: ex
            );
        }
    }

    public bool IntentarResolverRutaRelativa(
        string rutaRaiz,
        string rutaRelativa,
        out string rutaCompleta) {
        rutaCompleta = "";

        if (string.IsNullOrWhiteSpace(rutaRelativa) || Path.IsPathRooted(rutaRelativa)) {
            return false;
        }

        try {
            string raizNormalizada = Path.GetFullPath(rutaRaiz);
            string rutaNormalizada = Path.GetFullPath(rutaRelativa, raizNormalizada);
            string rutaRelativaNormalizada = Path.GetRelativePath(raizNormalizada, rutaNormalizada);

            if (Path.IsPathRooted(rutaRelativaNormalizada) ||
                rutaRelativaNormalizada.Equals("..", StringComparison.Ordinal) ||
                rutaRelativaNormalizada.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                rutaRelativaNormalizada.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal)) {
                return false;
            }

            rutaCompleta = rutaNormalizada;
            return true;
        } catch (ArgumentException) {
            return false;
        } catch (NotSupportedException) {
            return false;
        } catch (PathTooLongException) {
            return false;
        }
    }

    private ResultadoSeleccionEvaluacionCpp ResolverSolucionParaEvaluacion(
        string rutaPractica) {
        string rutaMarcador = Path.Combine(rutaPractica, NombreArchivoSeleccion);
        bool existeMarcador;

        try {
            File.GetAttributes(rutaMarcador);
            existeMarcador = true;
        } catch (FileNotFoundException) {
            existeMarcador = false;
        } catch (DirectoryNotFoundException) {
            existeMarcador = false;
        } catch (UnauthorizedAccessException ex) {
            return new ResultadoSeleccionEvaluacionCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.MarcadorIlegible,
                UsaSeleccionGuardada = true,
                Error = ex
            };
        } catch (IOException ex) {
            return new ResultadoSeleccionEvaluacionCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.MarcadorIlegible,
                UsaSeleccionGuardada = true,
                Error = ex
            };
        }

        if (!existeMarcador) {
            string[] soluciones = ObtenerSolucionesOrdenadas(rutaPractica);

            return soluciones.Length switch {
                0 => new ResultadoSeleccionEvaluacionCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.SolucionInexistente
                },
                1 => new ResultadoSeleccionEvaluacionCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.Exitosa,
                    RutaSolucion = Path.GetFullPath(soluciones[0])
                },
                _ => new ResultadoSeleccionEvaluacionCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.SolucionAmbigua
                }
            };
        }

        string rutaRelativa;

        try {
            rutaRelativa = LeerTextoAcotado(
                rutaMarcador,
                CaracteresMaximosSeleccion
            ).Trim();
        } catch (Exception ex) when (
            ex is IOException or UnauthorizedAccessException or DecoderFallbackException) {
            return new ResultadoSeleccionEvaluacionCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.MarcadorIlegible,
                UsaSeleccionGuardada = true,
                Error = ex
            };
        }

        if (string.IsNullOrWhiteSpace(rutaRelativa) ||
            rutaRelativa.Contains('\r', StringComparison.Ordinal) ||
            rutaRelativa.Contains('\n', StringComparison.Ordinal) ||
            Path.IsPathRooted(rutaRelativa) ||
            !Path.GetExtension(rutaRelativa).Equals(".sln", StringComparison.OrdinalIgnoreCase)) {
            return new ResultadoSeleccionEvaluacionCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.MarcadorInvalido,
                UsaSeleccionGuardada = true
            };
        }

        if (!IntentarResolverRutaRelativa(
            rutaPractica,
            rutaRelativa,
            out string rutaSolucion)) {
            return new ResultadoSeleccionEvaluacionCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.SolucionFueraDePractica,
                UsaSeleccionGuardada = true
            };
        }

        if (!File.Exists(rutaSolucion)) {
            return new ResultadoSeleccionEvaluacionCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.SolucionInexistente,
                UsaSeleccionGuardada = true
            };
        }

        return new ResultadoSeleccionEvaluacionCpp {
            Estado = EstadoResolucionProyectoEvaluacionCpp.Exitosa,
            RutaSolucion = rutaSolucion,
            UsaSeleccionGuardada = true
        };
    }

    private ResultadoProyectoReferenciadoCpp ResolverProyectoCppReferenciado(
        string rutaPractica,
        string rutaSolucion) {
        if (!DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
            rutaPractica,
            rutaSolucion)) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.SolucionFueraDePractica
            };
        }

        if (new FileInfo(rutaSolucion).Length > BytesMaximosSolucion) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoInvalido,
                Error = new InvalidDataException(
                    "El archivo de solución excede el tamaño admitido para evaluación."
                )
            };
        }

        string directorioSolucion = Path.GetDirectoryName(rutaSolucion)!;
        string[] referencias = ExtraerReferenciasProyectoCpp(rutaSolucion);

        if (referencias.Length == 0) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.SolucionSinProyectoCpp
            };
        }

        List<string> proyectos = new();

        foreach (string referencia in referencias) {
            string rutaProyecto;

            try {
                rutaProyecto = Path.GetFullPath(referencia, directorioSolucion);
            } catch (Exception ex) when (
                ex is ArgumentException or NotSupportedException or PathTooLongException) {
                return new ResultadoProyectoReferenciadoCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoFueraDePractica,
                    Error = ex
                };
            }

            if (!DirectorioTemporalEvaluacionCpp.EstaDentroDe(rutaPractica, rutaProyecto)) {
                return new ResultadoProyectoReferenciadoCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoFueraDePractica
                };
            }

            if (!DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                rutaPractica,
                rutaProyecto)) {
                return new ResultadoProyectoReferenciadoCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoFueraDePractica
                };
            }

            if (!File.Exists(rutaProyecto)) {
                return new ResultadoProyectoReferenciadoCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoInexistente
                };
            }

            proyectos.Add(rutaProyecto);
        }

        string[] proyectosAplicacion;

        try {
            proyectosAplicacion = proyectos
                .Where(EsProyectoAplicacion)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(Path.GetFileName, StringComparer.Ordinal)
                .ToArray();
        } catch (XmlException ex) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoInvalido,
                Error = ex
            };
        }

        if (proyectosAplicacion.Length == 0) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoNoEjecutable
            };
        }

        if (proyectosAplicacion.Length == 1) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.Exitosa,
                RutaProyectoCpp = proyectosAplicacion[0]
            };
        }

        string nombreSolucion = Path.GetFileNameWithoutExtension(rutaSolucion);
        string[] coincidencias = proyectosAplicacion
            .Where(proyecto => Path
                .GetFileNameWithoutExtension(proyecto)
                .Equals(nombreSolucion, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return coincidencias.Length == 1
            ? new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.Exitosa,
                RutaProyectoCpp = coincidencias[0]
            }
            : new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoAmbiguo
            };
    }

    private static string[] ExtraerReferenciasProyectoCpp(string rutaSolucion) {
        List<string> referencias = new();

        foreach (string linea in File.ReadLines(rutaSolucion)) {
            string[] campos = linea.Split('"');

            if (campos.Length <= 5 ||
                !linea.TrimStart().StartsWith("Project(", StringComparison.Ordinal)) {
                continue;
            }

            string referencia = campos[5];

            if (Path.GetExtension(referencia).Equals(".vcxproj", StringComparison.OrdinalIgnoreCase)) {
                referencias.Add(referencia);
            }
        }

        return referencias
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(referencia => referencia, StringComparer.OrdinalIgnoreCase)
            .ThenBy(referencia => referencia, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool EsProyectoAplicacion(string rutaProyecto) {
        XmlReaderSettings configuracionXml = new() {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using XmlReader lector = XmlReader.Create(rutaProyecto, configuracionXml);
        XDocument proyecto = XDocument.Load(lector, LoadOptions.None);

        return proyecto
            .Descendants()
            .Where(elemento => elemento.Name.LocalName.Equals(
                "ConfigurationType",
                StringComparison.Ordinal
            ))
            .Any(elemento => elemento.Value.Trim().Equals(
                "Application",
                StringComparison.OrdinalIgnoreCase
            ));
    }

    private static string LeerTextoAcotado(string rutaArchivo, int caracteresMaximos) {
        using FileStream flujo = new(
            rutaArchivo,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );
        using StreamReader lector = new(
            flujo,
            new UTF8Encoding(false, true),
            detectEncodingFromByteOrderMarks: true
        );

        char[] contenido = new char[caracteresMaximos + 1];
        int total = 0;

        while (total < contenido.Length) {
            int leidos = lector.Read(contenido, total, contenido.Length - total);

            if (leidos == 0) {
                break;
            }

            total += leidos;
        }

        if (total > caracteresMaximos || lector.Peek() >= 0) {
            throw new InvalidDataException(
                "El archivo de selección de solución excede el tamaño admitido."
            );
        }

        return new string(contenido, 0, total);
    }

    private static ResultadoResolucionProyectoEvaluacionCpp CrearResultadoResolucion(
        EstadoResolucionProyectoEvaluacionCpp estado,
        string rutaSolucion = "",
        string rutaProyectoCpp = "",
        bool usaSeleccionGuardada = false,
        Exception? error = null) {
        return new ResultadoResolucionProyectoEvaluacionCpp {
            Estado = estado,
            RutaSolucion = rutaSolucion,
            RutaProyectoCpp = rutaProyectoCpp,
            UsaSeleccionGuardada = usaSeleccionGuardada,
            Error = error
        };
    }

    private sealed class ResultadoSeleccionEvaluacionCpp {
        public EstadoResolucionProyectoEvaluacionCpp Estado { get; init; }

        public string RutaSolucion { get; init; } = "";

        public bool UsaSeleccionGuardada { get; init; }

        public Exception? Error { get; init; }
    }

    private sealed class ResultadoProyectoReferenciadoCpp {
        public EstadoResolucionProyectoEvaluacionCpp Estado { get; init; }

        public string RutaProyectoCpp { get; init; } = "";

        public Exception? Error { get; init; }
    }
}
