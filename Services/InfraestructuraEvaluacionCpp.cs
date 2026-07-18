namespace EndForge.Services;

public enum EstadoResolucionProyectoEvaluacionCpp {
    Exitosa,
    CarpetaInexistente,
    MarcadorIlegible,
    MarcadorInvalido,
    SolucionInexistente,
    SolucionAmbigua,
    SolucionFueraDePractica,
    SolucionSinProyectoCpp,
    ProyectoInexistente,
    ProyectoFueraDePractica,
    ProyectoInvalido,
    ProyectoNoEjecutable,
    ProyectoAmbiguo,
    ErrorLectura
}

public sealed class ResultadoResolucionProyectoEvaluacionCpp {
    public EstadoResolucionProyectoEvaluacionCpp Estado { get; init; }

    public string RutaSolucion { get; init; } = "";

    public string RutaProyectoCpp { get; init; } = "";

    public bool UsaSeleccionGuardada { get; init; }

    public Exception? Error { get; init; }

    public bool EsExitosa => Estado == EstadoResolucionProyectoEvaluacionCpp.Exitosa;
}

public sealed class SolicitudCompilacionCpp {
    public string RutaPractica { get; init; } = "";

    public string Configuracion { get; init; } = "Debug";

    public string Plataforma { get; init; } = "x64";
}

public enum EstadoCompilacionCpp {
    Exitosa,
    ProyectoNoDisponible,
    ConfiguracionNoDisponible,
    MsBuildNoDisponible,
    Cancelada,
    TiempoExcedido,
    SalidaExcesiva,
    ErrorInicio,
    EntornoCompilacionNoDisponible,
    ErrorCompilacion,
    EjecutableNoGenerado,
    EjecutableAmbiguo,
    EjecutableFueraDeDirectorioTemporal,
    ErrorInfraestructura
}

public sealed class ResultadoCompilacionCpp {
    public EstadoCompilacionCpp Estado { get; init; }

    public ResultadoResolucionProyectoEvaluacionCpp? ResolucionProyecto { get; init; }

    public string SalidaEstandar { get; init; } = "";

    public string SalidaError { get; init; } = "";

    public int? CodigoSalida { get; init; }

    public bool SalidaTruncada { get; init; }

    public Exception? Error { get; init; }

    public SesionCompilacionCpp? Sesion { get; init; }

    public bool EsExitosa => Estado == EstadoCompilacionCpp.Exitosa && Sesion != null;
}

public sealed class SesionCompilacionCpp : IDisposable {
    private readonly string directorioRaizTemporal;
    private readonly string identificadorPropiedad;
    private int liberada;

    internal SesionCompilacionCpp(
        string rutaPractica,
        string rutaSolucion,
        string rutaProyectoCpp,
        string rutaEjecutable,
        string directorioArtefactos,
        string directorioRaizTemporal,
        string identificadorPropiedad) {
        RutaPractica = rutaPractica;
        RutaSolucion = rutaSolucion;
        RutaProyectoCpp = rutaProyectoCpp;
        RutaEjecutable = rutaEjecutable;
        DirectorioArtefactos = directorioArtefactos;
        this.directorioRaizTemporal = directorioRaizTemporal;
        this.identificadorPropiedad = identificadorPropiedad;
    }

    public string RutaPractica { get; }

    public string RutaSolucion { get; }

    public string RutaProyectoCpp { get; }

    public string RutaEjecutable { get; }

    public string DirectorioArtefactos { get; }

    public bool EstaLiberada => Volatile.Read(ref liberada) != 0;

    internal bool IntentarObtenerContextoEjecucion(
        out string rutaEjecutable,
        out string rutaPractica) {
        rutaEjecutable = "";
        rutaPractica = "";

        if (EstaLiberada ||
            !DirectorioTemporalEvaluacionCpp.EsSesionPropia(
                directorioRaizTemporal,
                DirectorioArtefactos,
                identificadorPropiedad) ||
            !DirectorioTemporalEvaluacionCpp.EstaDentroDe(
                DirectorioArtefactos,
                RutaEjecutable) ||
            !DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                DirectorioArtefactos,
                RutaEjecutable)) {
            return false;
        }

        rutaEjecutable = RutaEjecutable;
        rutaPractica = RutaPractica;
        return true;
    }

    public void Dispose() {
        if (Interlocked.Exchange(ref liberada, 1) != 0) {
            return;
        }

        DirectorioTemporalEvaluacionCpp.EliminarSesionPropia(
            directorioRaizTemporal,
            DirectorioArtefactos,
            identificadorPropiedad
        );
    }
}

public enum EstadoEjecucionPruebaCpp {
    Exitosa,
    SesionNoDisponible,
    EjecutableInexistente,
    DirectorioTrabajoInexistente,
    Cancelada,
    TiempoTecnicoExcedido,
    SalidaExcesiva,
    CodigoSalidaNoCero,
    ErrorInicio,
    ErrorInfraestructura
}

public sealed class ResultadoEjecucionPruebaCpp {
    public EstadoEjecucionPruebaCpp Estado { get; init; }

    public string SalidaEstandar { get; init; } = "";

    public string SalidaError { get; init; } = "";

    public int? CodigoSalida { get; init; }

    public bool SalidaTruncada { get; init; }

    public TimeSpan Duracion { get; init; }

    public Exception? Error { get; init; }

    public bool EjecucionFinalizada =>
        Estado == EstadoEjecucionPruebaCpp.Exitosa ||
        Estado == EstadoEjecucionPruebaCpp.CodigoSalidaNoCero;
}

internal static class DirectorioTemporalEvaluacionCpp {
    private const string NombreArchivoPropiedad = ".endforge-evaluation-owned";

    public static (string DirectorioRaiz, string DirectorioSesion, string Identificador)
        CrearSesion() {
        string directorioRaiz = Path.GetFullPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EndForge",
            "Temp",
            "Evaluaciones"
        ));

        Directory.CreateDirectory(directorioRaiz);

        while (true) {
            string identificador = Guid.NewGuid().ToString("N");
            string directorioSesion = Path.Combine(
                directorioRaiz,
                $"compilacion-{identificador}"
            );
            bool directorioCreado = false;

            if (Directory.Exists(directorioSesion) || File.Exists(directorioSesion)) {
                continue;
            }

            try {
                Directory.CreateDirectory(directorioSesion);
                directorioCreado = true;

                using FileStream propiedad = new(
                    Path.Combine(directorioSesion, NombreArchivoPropiedad),
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None
                );
                using StreamWriter escritor = new(propiedad);
                escritor.Write(identificador);

                return (directorioRaiz, directorioSesion, identificador);
            } catch (IOException) {
                if (directorioCreado) {
                    EliminarDirectorioRecienCreado(directorioRaiz, directorioSesion);
                }

                // La colisión es extremadamente improbable; se genera otro identificador.
            } catch (Exception) {
                if (directorioCreado) {
                    EliminarDirectorioRecienCreado(directorioRaiz, directorioSesion);
                }

                throw;
            }
        }
    }

    public static bool EstaDentroDe(string rutaRaiz, string rutaCandidata) {
        try {
            string raiz = Path.GetFullPath(rutaRaiz);
            string candidata = Path.GetFullPath(rutaCandidata);
            string relativa = Path.GetRelativePath(raiz, candidata);

            return !Path.IsPathRooted(relativa) &&
                !relativa.Equals("..", StringComparison.Ordinal) &&
                !relativa.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                !relativa.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);
        } catch (ArgumentException) {
            return false;
        } catch (NotSupportedException) {
            return false;
        } catch (PathTooLongException) {
            return false;
        }
    }

    public static bool EsSesionPropia(
        string directorioRaiz,
        string directorioSesion,
        string identificador) {
        try {
            string sesionNormalizada = Path.GetFullPath(directorioSesion);
            string nombreEsperado = $"compilacion-{identificador}";

            if (!EstaDentroDe(directorioRaiz, sesionNormalizada) ||
                !Path.GetFileName(sesionNormalizada).Equals(nombreEsperado, StringComparison.Ordinal) ||
                !Directory.Exists(sesionNormalizada) ||
                File.GetAttributes(sesionNormalizada).HasFlag(FileAttributes.ReparsePoint)) {
                return false;
            }

            string rutaPropiedad = Path.Combine(sesionNormalizada, NombreArchivoPropiedad);

            return File.Exists(rutaPropiedad) &&
                File.ReadAllText(rutaPropiedad).Equals(identificador, StringComparison.Ordinal);
        } catch (Exception) {
            return false;
        }
    }

    public static bool EsRutaSinPuntosDeReanalisis(
        string rutaRaiz,
        string rutaCandidata) {
        try {
            string raiz = Path.GetFullPath(rutaRaiz);
            string candidata = Path.GetFullPath(rutaCandidata);

            if (!EstaDentroDe(raiz, candidata) ||
                File.GetAttributes(raiz).HasFlag(FileAttributes.ReparsePoint)) {
                return false;
            }

            string relativa = Path.GetRelativePath(raiz, candidata);
            string actual = raiz;

            foreach (string segmento in relativa.Split(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries)) {
                actual = Path.Combine(actual, segmento);

                if ((Directory.Exists(actual) || File.Exists(actual)) &&
                    File.GetAttributes(actual).HasFlag(FileAttributes.ReparsePoint)) {
                    return false;
                }
            }

            return true;
        } catch (Exception) {
            return false;
        }
    }

    public static IEnumerable<string> EnumerarArchivosSinPuntosDeReanalisis(
        string directorioRaiz) {
        Stack<string> pendientes = new();
        pendientes.Push(Path.GetFullPath(directorioRaiz));

        while (pendientes.Count > 0) {
            string directorio = pendientes.Pop();

            if (File.GetAttributes(directorio).HasFlag(FileAttributes.ReparsePoint)) {
                continue;
            }

            foreach (string entrada in Directory.EnumerateFileSystemEntries(
                directorio,
                "*",
                SearchOption.TopDirectoryOnly)) {
                FileAttributes atributos = File.GetAttributes(entrada);

                if (atributos.HasFlag(FileAttributes.Directory)) {
                    if (!atributos.HasFlag(FileAttributes.ReparsePoint)) {
                        pendientes.Push(entrada);
                    }
                } else if (!atributos.HasFlag(FileAttributes.ReparsePoint)) {
                    yield return entrada;
                }
            }
        }
    }

    public static void EliminarSesionPropia(
        string directorioRaiz,
        string directorioSesion,
        string identificador) {
        if (!EsSesionPropia(directorioRaiz, directorioSesion, identificador)) {
            return;
        }

        try {
            EliminarArbolSinSeguirPuntosDeReanalisis(directorioSesion);
        } catch (Exception) {
            // La limpieza nunca debe ocultar el resultado de la evaluación.
        }
    }

    private static void EliminarDirectorioRecienCreado(
        string directorioRaiz,
        string directorioSesion) {
        try {
            if (Directory.Exists(directorioSesion) &&
                EstaDentroDe(directorioRaiz, directorioSesion) &&
                !File.GetAttributes(directorioSesion).HasFlag(FileAttributes.ReparsePoint)) {
                EliminarArbolSinSeguirPuntosDeReanalisis(directorioSesion);
            }
        } catch (Exception) {
            // Se preserva el error de creación original.
        }
    }

    private static void EliminarArbolSinSeguirPuntosDeReanalisis(string directorio) {
        foreach (string entrada in Directory.EnumerateFileSystemEntries(
            directorio,
            "*",
            SearchOption.TopDirectoryOnly)) {
            FileAttributes atributos = File.GetAttributes(entrada);

            if (atributos.HasFlag(FileAttributes.Directory) &&
                !atributos.HasFlag(FileAttributes.ReparsePoint)) {
                EliminarArbolSinSeguirPuntosDeReanalisis(entrada);
                continue;
            }

            if (atributos.HasFlag(FileAttributes.Directory)) {
                Directory.Delete(entrada, recursive: false);
            } else {
                File.Delete(entrada);
            }
        }

        Directory.Delete(directorio, recursive: false);
    }
}
