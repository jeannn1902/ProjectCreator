namespace EndForge.Models;

public sealed class TemaCurso {
    public string Id { get; init; } = "";

    public int Numero { get; init; }

    public string Nombre { get; init; } = "";

    public string NombreCarpeta { get; init; } = "";

    public string Descripcion { get; init; } = "";

    public int TotalPracticasPlaneadas { get; init; }

    public IReadOnlyList<PracticaCurso> Practicas { get; init; } = Array.Empty<PracticaCurso>();

    public bool EsProximamente { get; init; }

    public string MensajeDisponibilidad { get; init; } = "";
}
