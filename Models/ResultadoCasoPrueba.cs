namespace EndForge.Models;

public sealed class ResultadoCasoPrueba {
    public string CasoPruebaId { get; init; } = "";

    public string Entrada { get; init; } = "";

    public string SalidaEsperada { get; init; } = "";

    public string SalidaObtenida { get; init; } = "";

    public bool Aprobado { get; init; }

    public int PuntosObtenidos { get; init; }

    public int PuntosMaximos { get; init; }

    public string Mensaje { get; init; } = "";

    public bool EsVisible { get; init; } = true;

    public bool EjecucionFinalizada { get; init; } = true;
}
