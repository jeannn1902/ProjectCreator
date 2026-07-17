namespace EndForge.Models;

public sealed class ProgresoCurso {
    public List<ProgresoPractica> Practicas { get; set; } = new();
}

public enum EstadoCargaProgreso {
    Exitosa,
    ArchivoInexistente,
    ArchivoVacio,
    ContenidoInvalido,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoCargaProgreso {
    public EstadoCargaProgreso Estado { get; init; }

    public ProgresoCurso Progreso { get; init; } = new();

    public int RegistrosInvalidos { get; init; }

    public Exception? Error { get; init; }

    public bool DatosDisponibles =>
        Estado == EstadoCargaProgreso.Exitosa ||
        Estado == EstadoCargaProgreso.ArchivoInexistente ||
        Estado == EstadoCargaProgreso.ArchivoVacio ||
        Estado == EstadoCargaProgreso.ContenidoInvalido;
}

public enum EstadoEscrituraProgreso {
    Exitosa,
    ContenidoInvalido,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoEscrituraProgreso {
    public EstadoEscrituraProgreso Estado { get; init; }

    public int RegistrosInvalidosIgnorados { get; init; }

    public Exception? Error { get; init; }

    public bool EsExitosa => Estado == EstadoEscrituraProgreso.Exitosa;
}
