namespace EndForge.Models;

public enum EstadoGradoCurso {
    Disponible,
    EnProgreso,
    ContenidoDisponibleCompletado,
    Completado,
    Proximamente
}

public sealed class GradoCurso {
    public string Id { get; init; } = "";

    public int Numero { get; init; }

    public string Nombre { get; init; } = "";

    public string Titulo => $"Grado {Numero} — {Nombre}";

    public string Descripcion { get; init; } = "";

    public EstadoGradoCurso Estado { get; init; }

    public IReadOnlyList<TemaCurso> Temas { get; init; } = Array.Empty<TemaCurso>();

    public int Porcentaje { get; init; }

    public int CantidadPracticasDisponibles { get; init; }

    public int CantidadPracticasCompletadas { get; init; }

    public int CantidadPracticasPlaneadas { get; init; }

    public bool EsContenidoDisponible { get; init; }
}
