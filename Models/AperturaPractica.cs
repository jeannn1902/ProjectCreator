namespace EndForge.Models;

public enum EstadoAperturaPractica {
    Exitosa,
    CarpetaInexistente,
    SolucionInexistente,
    ErrorApertura
}

public sealed class ResultadoAperturaPractica {
    public EstadoAperturaPractica Estado { get; init; }

    public string? RutaSolucion { get; init; }

    public Exception? Error { get; init; }
}
