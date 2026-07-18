namespace EndForge.Models;

public sealed class DefinicionEvaluacionPractica {
    public string PracticaId { get; init; } = "";

    public string NombrePractica { get; init; } = "";

    public string Objetivo { get; init; } = "";

    public string Descripcion { get; init; } = "";

    public string ContratoEntrada { get; init; } = "";

    public IReadOnlyList<string> CamposEntrada { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> ValidacionesRequeridas { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<CasoPrueba> CasosPrueba { get; init; } =
        Array.Empty<CasoPrueba>();

    public IReadOnlyList<CriterioEvaluacion> Criterios { get; init; } =
        Array.Empty<CriterioEvaluacion>();

    public int PuntosCasosPrueba => CasosPrueba.Sum(caso => caso.Puntos);

    public int PuntosMaximos => Criterios.Sum(criterio => criterio.PuntosMaximos);
}
