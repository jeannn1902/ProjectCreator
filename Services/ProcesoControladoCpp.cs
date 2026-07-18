using System.Diagnostics;
using System.Text;

namespace EndForge.Services;

internal enum EstadoProcesoControladoCpp {
    Exitosa,
    Cancelada,
    TiempoExcedido,
    SalidaExcesiva,
    ErrorInicio,
    ErrorEjecucion
}

internal sealed class SolicitudProcesoControladoCpp {
    public string Archivo { get; init; } = "";

    public string DirectorioTrabajo { get; init; } = "";

    public IReadOnlyList<string> Argumentos { get; init; } = Array.Empty<string>();

    public string? Entrada { get; init; }

    public TimeSpan TiempoMaximo { get; init; }

    public int LimiteCaracteresSalida { get; init; }

    public IReadOnlyDictionary<string, string?> VariablesEntorno { get; init; } =
        new Dictionary<string, string?>();
}

internal sealed class ResultadoProcesoControladoCpp {
    public EstadoProcesoControladoCpp Estado { get; init; }

    public string SalidaEstandar { get; init; } = "";

    public string SalidaError { get; init; } = "";

    public int? CodigoSalida { get; init; }

    public bool SalidaTruncada { get; init; }

    public TimeSpan Duracion { get; init; }

    public Exception? Error { get; init; }
}

internal static class ProcesoControladoCpp {
    private static readonly Encoding CodificacionProceso = new UTF8Encoding(false, false);

    public static async Task<ResultadoProcesoControladoCpp> EjecutarAsync(
        SolicitudProcesoControladoCpp solicitud,
        CancellationToken cancellationToken) {
        Stopwatch duracion = Stopwatch.StartNew();

        if (cancellationToken.IsCancellationRequested) {
            return CrearResultado(
                EstadoProcesoControladoCpp.Cancelada,
                duracion: duracion.Elapsed
            );
        }

        if (solicitud.TiempoMaximo <= TimeSpan.Zero) {
            return CrearResultado(
                EstadoProcesoControladoCpp.ErrorInicio,
                error: new ArgumentOutOfRangeException(
                    nameof(solicitud.TiempoMaximo),
                    "El tiempo máximo debe ser mayor que cero."
                ),
                duracion: duracion.Elapsed
            );
        }

        if (solicitud.LimiteCaracteresSalida <= 0) {
            return CrearResultado(
                EstadoProcesoControladoCpp.ErrorInicio,
                error: new ArgumentOutOfRangeException(
                    nameof(solicitud.LimiteCaracteresSalida),
                    "El límite de salida debe ser mayor que cero."
                ),
                duracion: duracion.Elapsed
            );
        }

        using CancellationTokenSource tiempoMaximo = new(solicitud.TiempoMaximo);
        using CancellationTokenSource salidaExcesiva = new();
        using CancellationTokenSource cancelacionCombinada =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                tiempoMaximo.Token,
                salidaExcesiva.Token
            );

        CapturaSalidaProcesoCpp captura = new(
            solicitud.LimiteCaracteresSalida,
            salidaExcesiva
        );

        using Process proceso = new() {
            StartInfo = CrearInformacionInicio(solicitud),
            EnableRaisingEvents = false
        };

        Task lecturaSalida = Task.CompletedTask;
        Task lecturaError = Task.CompletedTask;
        Task escrituraEntrada = Task.CompletedTask;
        bool iniciado = false;

        try {
            iniciado = proceso.Start();

            if (!iniciado) {
                return CrearResultado(
                    EstadoProcesoControladoCpp.ErrorInicio,
                    error: new InvalidOperationException("No fue posible iniciar el proceso."),
                    duracion: duracion.Elapsed
                );
            }

            lecturaSalida = CapturarAsync(
                proceso.StandardOutput,
                captura,
                esError: false
            );
            lecturaError = CapturarAsync(
                proceso.StandardError,
                captura,
                esError: true
            );

            if (solicitud.Entrada != null) {
                escrituraEntrada = EscribirEntradaAsync(
                    proceso.StandardInput,
                    solicitud.Entrada,
                    cancelacionCombinada.Token
                );
            }

            try {
                await proceso
                    .WaitForExitAsync(cancelacionCombinada.Token)
                    .ConfigureAwait(false);
            } catch (OperationCanceledException) {
                await DetenerProcesoPropioAsync(proceso).ConfigureAwait(false);

                EstadoProcesoControladoCpp estadoCancelacion =
                    cancellationToken.IsCancellationRequested
                        ? EstadoProcesoControladoCpp.Cancelada
                        : captura.LimiteExcedido
                            ? EstadoProcesoControladoCpp.SalidaExcesiva
                            : EstadoProcesoControladoCpp.TiempoExcedido;

                await CompletarTareasDeFlujoAsync(
                    escrituraEntrada,
                    lecturaSalida,
                    lecturaError
                ).ConfigureAwait(false);

                return CrearResultado(
                    estadoCancelacion,
                    captura,
                    duracion: duracion.Elapsed
                );
            }

            await CompletarTareasDeFlujoAsync(
                escrituraEntrada,
                lecturaSalida,
                lecturaError
            ).ConfigureAwait(false);

            return CrearResultado(
                EstadoProcesoControladoCpp.Exitosa,
                captura,
                proceso.ExitCode,
                duracion: duracion.Elapsed
            );
        } catch (Exception ex) {
            if (iniciado) {
                await DetenerProcesoPropioAsync(proceso).ConfigureAwait(false);
                await CompletarTareasDeFlujoAsync(
                    escrituraEntrada,
                    lecturaSalida,
                    lecturaError
                ).ConfigureAwait(false);
            }

            return CrearResultado(
                iniciado
                    ? EstadoProcesoControladoCpp.ErrorEjecucion
                    : EstadoProcesoControladoCpp.ErrorInicio,
                captura,
                error: ex,
                duracion: duracion.Elapsed
            );
        }
    }

    private static ProcessStartInfo CrearInformacionInicio(
        SolicitudProcesoControladoCpp solicitud) {
        ProcessStartInfo informacion = new() {
            FileName = solicitud.Archivo,
            WorkingDirectory = solicitud.DirectorioTrabajo,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardInput = solicitud.Entrada != null,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = CodificacionProceso,
            StandardErrorEncoding = CodificacionProceso
        };

        if (solicitud.Entrada != null) {
            informacion.StandardInputEncoding = CodificacionProceso;
        }

        foreach (string argumento in solicitud.Argumentos) {
            informacion.ArgumentList.Add(argumento);
        }

        foreach ((string nombre, string? valor) in solicitud.VariablesEntorno) {
            if (valor == null) {
                informacion.Environment.Remove(nombre);
            } else {
                informacion.Environment[nombre] = valor;
            }
        }

        return informacion;
    }

    private static async Task CapturarAsync(
        StreamReader lector,
        CapturaSalidaProcesoCpp captura,
        bool esError) {
        char[] buffer = new char[4096];

        try {
            while (true) {
                int leidos = await lector
                    .ReadAsync(buffer.AsMemory())
                    .ConfigureAwait(false);

                if (leidos == 0) {
                    return;
                }

                captura.Agregar(buffer.AsSpan(0, leidos), esError);
            }
        } catch (IOException) {
            // El stream puede cerrarse al cancelar o finalizar el proceso.
        } catch (ObjectDisposedException) {
            // El proceso ya terminó y liberó su stream.
        }
    }

    private static async Task EscribirEntradaAsync(
        StreamWriter escritor,
        string entrada,
        CancellationToken cancellationToken) {
        try {
            await escritor
                .WriteAsync(entrada.AsMemory(), cancellationToken)
                .ConfigureAwait(false);
            await escritor.FlushAsync(cancellationToken).ConfigureAwait(false);
        } catch (OperationCanceledException) {
            // La cancelación se clasifica por el proceso principal.
        } catch (IOException) {
            // El programa puede cerrar stdin antes de consumir toda la entrada.
        } catch (ObjectDisposedException) {
            // El proceso terminó antes de completar la escritura.
        } finally {
            try {
                escritor.Close();
            } catch (Exception) {
                // Cerrar stdin no debe ocultar el resultado del proceso.
            }
        }
    }

    private static async Task CompletarTareasDeFlujoAsync(params Task[] tareas) {
        try {
            await Task.WhenAll(tareas).ConfigureAwait(false);
        } catch (Exception) {
            // Los streams pueden cerrarse durante la cancelación del proceso.
        }
    }

    private static async Task DetenerProcesoPropioAsync(Process proceso) {
        try {
            if (!proceso.HasExited) {
                proceso.Kill(entireProcessTree: true);
            }
        } catch (InvalidOperationException) {
            return;
        } catch (System.ComponentModel.Win32Exception) {
            // Se intenta esperar igualmente por si el proceso ya está terminando.
        }

        using CancellationTokenSource esperaLimpieza = new(TimeSpan.FromSeconds(5));

        try {
            await proceso
                .WaitForExitAsync(esperaLimpieza.Token)
                .ConfigureAwait(false);
        } catch (OperationCanceledException) {
            try {
                if (!proceso.HasExited) {
                    proceso.Kill(entireProcessTree: true);
                }
            } catch (Exception) {
                // Nunca se finaliza un proceso distinto del iniciado por EndForge.
            }
        } catch (InvalidOperationException) {
            // El proceso ya no está asociado a un identificador activo.
        }
    }

    private static ResultadoProcesoControladoCpp CrearResultado(
        EstadoProcesoControladoCpp estado,
        CapturaSalidaProcesoCpp? captura = null,
        int? codigoSalida = null,
        Exception? error = null,
        TimeSpan duracion = default) {
        return new ResultadoProcesoControladoCpp {
            Estado = estado,
            SalidaEstandar = captura?.SalidaEstandar ?? "",
            SalidaError = captura?.SalidaError ?? "",
            CodigoSalida = codigoSalida,
            SalidaTruncada = captura?.LimiteExcedido ?? false,
            Error = error,
            Duracion = duracion
        };
    }
}

internal sealed class CapturaSalidaProcesoCpp {
    private readonly object sincronizacion = new();
    private readonly int limiteCaracteres;
    private readonly CancellationTokenSource cancelacionPorLimite;
    private readonly StringBuilder salidaEstandar = new();
    private readonly StringBuilder salidaError = new();
    private int caracteresCapturados;
    private bool limiteExcedido;

    public CapturaSalidaProcesoCpp(
        int limiteCaracteres,
        CancellationTokenSource cancelacionPorLimite) {
        this.limiteCaracteres = limiteCaracteres;
        this.cancelacionPorLimite = cancelacionPorLimite;
    }

    public bool LimiteExcedido {
        get {
            lock (sincronizacion) {
                return limiteExcedido;
            }
        }
    }

    public string SalidaEstandar {
        get {
            lock (sincronizacion) {
                return salidaEstandar.ToString();
            }
        }
    }

    public string SalidaError {
        get {
            lock (sincronizacion) {
                return salidaError.ToString();
            }
        }
    }

    public void Agregar(ReadOnlySpan<char> contenido, bool esError) {
        bool cancelar = false;

        lock (sincronizacion) {
            if (limiteExcedido) {
                return;
            }

            int disponibles = limiteCaracteres - caracteresCapturados;
            int cantidad = Math.Min(disponibles, contenido.Length);

            if (cantidad > 0) {
                StringBuilder destino = esError ? salidaError : salidaEstandar;
                destino.Append(contenido[..cantidad]);
                caracteresCapturados += cantidad;
            }

            if (cantidad < contenido.Length) {
                limiteExcedido = true;
                cancelar = true;
            }
        }

        if (cancelar) {
            try {
                cancelacionPorLimite.Cancel();
            } catch (ObjectDisposedException) {
                // El proceso ya finalizó.
            }
        }
    }
}
