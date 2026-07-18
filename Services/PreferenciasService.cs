using System.Security;
using System.Text;
using System.Text.Json;
using EndForge.Models;

namespace EndForge.Services;

public sealed class PreferenciasService {
    private readonly string carpetaDatos;
    private readonly JsonSerializerOptions opcionesJson = new() {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public string RutaPreferencias { get; }

    public PreferenciasService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EndForge")) {
    }

    internal PreferenciasService(string carpetaDatos) {
        this.carpetaDatos = carpetaDatos;
        RutaPreferencias = Path.Combine(carpetaDatos, "preferencias.json");
    }

    public ResultadoCargaPreferencias Cargar() {
        string contenido;

        try {
            contenido = File.ReadAllText(RutaPreferencias, Encoding.UTF8);
        } catch (FileNotFoundException ex) {
            return CrearCarga(EstadoCargaPreferencias.ArchivoInexistente, error: ex);
        } catch (DirectoryNotFoundException ex) {
            return CrearCarga(EstadoCargaPreferencias.ArchivoInexistente, error: ex);
        } catch (UnauthorizedAccessException ex) {
            return CrearCarga(EstadoCargaPreferencias.PermisosInsuficientes, error: ex);
        } catch (SecurityException ex) {
            return CrearCarga(EstadoCargaPreferencias.PermisosInsuficientes, error: ex);
        } catch (IOException ex) {
            return CrearCarga(EstadoCargaPreferencias.ErrorIo, error: ex);
        } catch (Exception ex) {
            return CrearCarga(EstadoCargaPreferencias.ErrorIo, error: ex);
        }

        if (string.IsNullOrWhiteSpace(contenido)) {
            return CrearCarga(EstadoCargaPreferencias.ArchivoVacio);
        }

        try {
            PreferenciasAplicacion? preferencias =
                JsonSerializer.Deserialize<PreferenciasAplicacion>(contenido, opcionesJson);

            if (preferencias is null || preferencias.Version <= 0) {
                return CrearCarga(EstadoCargaPreferencias.ContenidoInvalido);
            }

            if (preferencias.Version != 1) {
                return CrearCarga(EstadoCargaPreferencias.VersionNoCompatible);
            }

            return CrearCarga(EstadoCargaPreferencias.Exitosa, preferencias);
        } catch (JsonException ex) {
            return CrearCarga(EstadoCargaPreferencias.ContenidoInvalido, error: ex);
        } catch (Exception ex) {
            return CrearCarga(EstadoCargaPreferencias.ContenidoInvalido, error: ex);
        }
    }

    public ResultadoEscrituraPreferencias Guardar(PreferenciasAplicacion preferencias) {
        ArgumentNullException.ThrowIfNull(preferencias);
        ResultadoCargaPreferencias carga = Cargar();

        if (!carga.EsRecuperable) {
            return new ResultadoEscrituraPreferencias {
                Estado = carga.Estado == EstadoCargaPreferencias.PermisosInsuficientes
                    ? EstadoEscrituraPreferencias.PermisosInsuficientes
                    : carga.Estado == EstadoCargaPreferencias.VersionNoCompatible
                        ? EstadoEscrituraPreferencias.VersionNoCompatible
                    : carga.Estado == EstadoCargaPreferencias.ContenidoInvalido
                        ? EstadoEscrituraPreferencias.ContenidoInvalido
                        : EstadoEscrituraPreferencias.ErrorIo,
                Error = carga.Error
            };
        }

        string? rutaTemporal = null;

        try {
            Directory.CreateDirectory(carpetaDatos);
            rutaTemporal = Path.Combine(
                carpetaDatos,
                $".preferencias-{Guid.NewGuid():N}.tmp");
            PreferenciasAplicacion normalizadas = new() {
                Version = 1,
                MostrarTiemposOrientativos = preferencias.MostrarTiemposOrientativos
            };
            string contenido = JsonSerializer.Serialize(normalizadas, opcionesJson);
            File.WriteAllText(rutaTemporal, contenido, new UTF8Encoding(false));

            if (File.Exists(RutaPreferencias)) {
                File.Replace(rutaTemporal, RutaPreferencias, null);
            } else {
                File.Move(rutaTemporal, RutaPreferencias);
            }

            return new ResultadoEscrituraPreferencias {
                Estado = EstadoEscrituraPreferencias.Exitosa
            };
        } catch (UnauthorizedAccessException ex) {
            LimpiarTemporal(rutaTemporal);
            return CrearEscritura(EstadoEscrituraPreferencias.PermisosInsuficientes, ex);
        } catch (SecurityException ex) {
            LimpiarTemporal(rutaTemporal);
            return CrearEscritura(EstadoEscrituraPreferencias.PermisosInsuficientes, ex);
        } catch (IOException ex) {
            LimpiarTemporal(rutaTemporal);
            return CrearEscritura(EstadoEscrituraPreferencias.ErrorIo, ex);
        } catch (Exception ex) {
            LimpiarTemporal(rutaTemporal);
            return CrearEscritura(EstadoEscrituraPreferencias.ErrorIo, ex);
        }
    }

    private static ResultadoCargaPreferencias CrearCarga(
        EstadoCargaPreferencias estado,
        PreferenciasAplicacion? preferencias = null,
        Exception? error = null) {
        return new ResultadoCargaPreferencias {
            Estado = estado,
            Preferencias = preferencias ?? new PreferenciasAplicacion(),
            Error = error
        };
    }

    private static ResultadoEscrituraPreferencias CrearEscritura(
        EstadoEscrituraPreferencias estado,
        Exception error) {
        return new ResultadoEscrituraPreferencias {
            Estado = estado,
            Error = error
        };
    }

    private static void LimpiarTemporal(string? rutaTemporal) {
        if (string.IsNullOrWhiteSpace(rutaTemporal)) {
            return;
        }

        try {
            if (File.Exists(rutaTemporal)) {
                File.Delete(rutaTemporal);
            }
        } catch (Exception) {
            // El error original de escritura tiene prioridad sobre la limpieza.
        }
    }
}
