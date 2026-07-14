namespace EndForge.Models;

public sealed class ResultadoValidacionNombrePractica {
    public bool EsValido { get; init; }
    public string MensajeError { get; init; } = "";
    public string NombreNormalizado { get; init; } = "";
}
