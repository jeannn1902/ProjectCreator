namespace EndForge.Models;

public sealed class HistorialEvaluaciones {
    public int Version { get; init; } = 1;

    public IReadOnlyList<HistorialPractica> Practicas { get; init; } =
        Array.Empty<HistorialPractica>();
}

public sealed class HistorialPractica {
    public string PracticaId { get; init; } = "";

    public int TotalIntentos { get; init; }

    public int? MejorCalificacion { get; init; }

    public int? UltimaCalificacion { get; init; }

    public DateTimeOffset? FechaUltimoIntento { get; init; }

    public IReadOnlyList<IntentoPractica> Intentos { get; init; } =
        Array.Empty<IntentoPractica>();
}

public enum EstadoCargaHistorialEvaluaciones {
    Exitosa,
    ArchivoInexistente,
    ArchivoVacio,
    ContenidoParcialmenteInvalido,
    ContenidoIrrecuperable,
    VersionNoCompatible,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoCargaHistorialEvaluaciones {
    public EstadoCargaHistorialEvaluaciones Estado { get; init; }

    public HistorialEvaluaciones Historial { get; init; } = new();

    public int RegistrosInvalidos { get; init; }

    public Exception? Error { get; init; }

    public bool DatosDisponibles =>
        Estado == EstadoCargaHistorialEvaluaciones.Exitosa ||
        Estado == EstadoCargaHistorialEvaluaciones.ArchivoInexistente ||
        Estado == EstadoCargaHistorialEvaluaciones.ArchivoVacio ||
        Estado == EstadoCargaHistorialEvaluaciones.ContenidoParcialmenteInvalido;
}

public enum EstadoEscrituraHistorialEvaluaciones {
    Exitosa,
    IntentoInvalido,
    IntentoDuplicado,
    ContenidoIrrecuperable,
    VersionNoCompatible,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoEscrituraHistorialEvaluaciones {
    public EstadoEscrituraHistorialEvaluaciones Estado { get; init; }

    public HistorialPractica? HistorialActualizado { get; init; }

    public int RegistrosInvalidosIgnorados { get; init; }

    public Exception? Error { get; init; }

    public bool EsExitosa => Estado == EstadoEscrituraHistorialEvaluaciones.Exitosa;
}

public enum EstadoEliminacionHistorialEvaluaciones {
    Exitosa,
    HistorialInexistente,
    IdentificadorInvalido,
    ContenidoIrrecuperable,
    VersionNoCompatible,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoEliminacionHistorialEvaluaciones {
    public EstadoEliminacionHistorialEvaluaciones Estado { get; init; }

    public int RegistrosInvalidosIgnorados { get; init; }

    public Exception? Error { get; init; }

    public bool EsExitosa =>
        Estado == EstadoEliminacionHistorialEvaluaciones.Exitosa ||
        Estado == EstadoEliminacionHistorialEvaluaciones.HistorialInexistente;
}
