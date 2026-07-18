namespace EndForge.Models;

public enum TipoCriterioEvaluacion {
    Compilacion,
    CasoPrueba,
    Validacion,
    ConceptoRequerido,
    CalidadBasica
}

public sealed class CriterioEvaluacion {
    public string Id { get; init; } = "";

    public string Nombre { get; init; } = "";

    public string Descripcion { get; init; } = "";

    public int PuntosMaximos { get; init; }

    public TipoCriterioEvaluacion Tipo { get; init; }
}
