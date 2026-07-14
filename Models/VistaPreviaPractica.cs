namespace EndForge.Models;

public enum EstadoVistaPreviaPractica {
    Vacia,
    Completa
}

public sealed class ResultadoVistaPreviaPractica {
    public EstadoVistaPreviaPractica Estado { get; init; }

    public string NombreFinal { get; init; } = "";
}
