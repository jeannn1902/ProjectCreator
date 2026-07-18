namespace EndForge.Services;

public sealed class EjecucionPruebasService {
    // Seguridad: esta ejecución local controla tiempo, salida y procesos propios,
    // pero no es un sandbox. Una futura evaluación en Azure deberá ejecutar cada
    // intento en un contenedor aislado, efímero y sin acceso a datos de EndForge.
    private const int LimiteCaracteresSalida = 64 * 1024;
    private static readonly TimeSpan TiempoTecnicoMaximo = TimeSpan.FromSeconds(5);

    public async Task<ResultadoEjecucionPruebaCpp> EjecutarCasoAsync(
        SesionCompilacionCpp sesion,
        string entrada,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(sesion);
        entrada ??= "";

        if (!sesion.IntentarObtenerContextoEjecucion(
            out string rutaEjecutable,
            out string rutaPractica)) {
            return new ResultadoEjecucionPruebaCpp {
                Estado = EstadoEjecucionPruebaCpp.SesionNoDisponible
            };
        }

        if (!File.Exists(rutaEjecutable)) {
            return new ResultadoEjecucionPruebaCpp {
                Estado = EstadoEjecucionPruebaCpp.EjecutableInexistente
            };
        }

        if (!Directory.Exists(rutaPractica)) {
            return new ResultadoEjecucionPruebaCpp {
                Estado = EstadoEjecucionPruebaCpp.DirectorioTrabajoInexistente
            };
        }

        ResultadoProcesoControladoCpp proceso =
            await ProcesoControladoCpp.EjecutarAsync(
                new SolicitudProcesoControladoCpp {
                    Archivo = rutaEjecutable,
                    DirectorioTrabajo = rutaPractica,
                    Entrada = entrada,
                    Argumentos = Array.Empty<string>(),
                    TiempoMaximo = TiempoTecnicoMaximo,
                    LimiteCaracteresSalida = LimiteCaracteresSalida
                },
                cancellationToken
            ).ConfigureAwait(false);

        EstadoEjecucionPruebaCpp estado = proceso.Estado switch {
            EstadoProcesoControladoCpp.Exitosa when proceso.CodigoSalida == 0 =>
                EstadoEjecucionPruebaCpp.Exitosa,
            EstadoProcesoControladoCpp.Exitosa =>
                EstadoEjecucionPruebaCpp.CodigoSalidaNoCero,
            EstadoProcesoControladoCpp.Cancelada =>
                EstadoEjecucionPruebaCpp.Cancelada,
            EstadoProcesoControladoCpp.TiempoExcedido =>
                EstadoEjecucionPruebaCpp.TiempoTecnicoExcedido,
            EstadoProcesoControladoCpp.SalidaExcesiva =>
                EstadoEjecucionPruebaCpp.SalidaExcesiva,
            EstadoProcesoControladoCpp.ErrorInicio =>
                EstadoEjecucionPruebaCpp.ErrorInicio,
            EstadoProcesoControladoCpp.ErrorEjecucion =>
                EstadoEjecucionPruebaCpp.ErrorInfraestructura,
            _ => EstadoEjecucionPruebaCpp.ErrorInfraestructura
        };

        return new ResultadoEjecucionPruebaCpp {
            Estado = estado,
            SalidaEstandar = proceso.SalidaEstandar,
            SalidaError = proceso.SalidaError,
            CodigoSalida = proceso.CodigoSalida,
            SalidaTruncada = proceso.SalidaTruncada,
            Duracion = proceso.Duracion,
            Error = proceso.Error
        };
    }
}
