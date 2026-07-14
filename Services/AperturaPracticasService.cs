using EndForge.Models;
using System.Diagnostics;

namespace EndForge.Services;

public sealed class AperturaPracticasService {
    public ResultadoAperturaPractica AbrirPractica(
        string rutaProyecto,
        Action? antesDeAbrir = null) {
        if (!Directory.Exists(rutaProyecto)) {
            return new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.CarpetaInexistente,
                Error = new DirectoryNotFoundException("La carpeta de esta práctica ya no existe.")
            };
        }

        string? rutaSolucion;

        try {
            // Regla determinista: nombre ordinal sin distinguir mayúsculas,
            // con orden ordinal como desempate.
            rutaSolucion = Directory
                .GetFiles(rutaProyecto, "*.sln", SearchOption.TopDirectoryOnly)
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(Path.GetFileName, StringComparer.Ordinal)
                .FirstOrDefault();
        } catch (Exception ex) {
            return new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.ErrorApertura,
                Error = ex
            };
        }

        if (rutaSolucion == null) {
            return new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.SolucionInexistente,
                Error = new FileNotFoundException("No se encontró ningún archivo .sln en la práctica.")
            };
        }

        try {
            antesDeAbrir?.Invoke();

            Process.Start(new ProcessStartInfo {
                FileName = rutaSolucion,
                UseShellExecute = true
            });

            return new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.Exitosa,
                RutaSolucion = rutaSolucion
            };
        } catch (Exception ex) {
            return new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.ErrorApertura,
                RutaSolucion = rutaSolucion,
                Error = ex
            };
        }
    }
}
