namespace EndForge.Models;

public sealed class ResultadoComparacionSalida {
    public bool EsCorrecta { get; init; }

    public bool CumpleEstructura { get; init; }

    public bool EsSalidaLegible { get; init; }

    public IReadOnlyList<string> TokensFaltantes { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> GruposAlternativosFaltantes { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<ResultadoValorNumericoComparado> ValoresNumericos { get; init; } =
        Array.Empty<ResultadoValorNumericoComparado>();

    public string Mensaje { get; init; } = "";
}

public sealed class ResultadoValorNumericoComparado {
    public string Nombre { get; init; } = "";

    public double ValorEsperado { get; init; }

    public double? ValorObtenido { get; init; }

    public double Tolerancia { get; init; }

    public bool Coincide { get; init; }

    public string EtiquetaEncontrada { get; init; } = "";
}
