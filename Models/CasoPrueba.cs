namespace EndForge.Models;

public sealed class CasoPrueba {
    public string Id { get; init; } = "";

    public string Nombre { get; init; } = "";

    public string Entrada { get; init; } = "";

    public string SalidaEsperada { get; init; } = "";

    public bool EsVisible { get; init; } = true;

    public int Puntos { get; init; }

    public bool ComparacionFlexible { get; init; }

    public string Descripcion { get; init; } = "";

    public IReadOnlyList<string> TokensObligatorios { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<GrupoTokensEsperados> GruposTokensAlternativos { get; init; } =
        Array.Empty<GrupoTokensEsperados>();

    public IReadOnlyList<ValorNumericoEsperado> ValoresNumericosEsperados { get; init; } =
        Array.Empty<ValorNumericoEsperado>();
}

public sealed class GrupoTokensEsperados {
    public string Nombre { get; init; } = "";

    public IReadOnlyList<string> Alternativas { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> EtiquetasAsociadas { get; init; } =
        Array.Empty<string>();
}

public sealed class ValorNumericoEsperado {
    public string Nombre { get; init; } = "";

    public double Valor { get; init; }

    public double Tolerancia { get; init; } = 0.01D;

    public IReadOnlyList<string> EtiquetasAlternativas { get; init; } =
        Array.Empty<string>();
}
