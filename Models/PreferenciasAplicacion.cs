namespace EndForge.Models;

public sealed class PreferenciasAplicacion {
    public int Version { get; set; } = 1;

    public bool MostrarTiemposOrientativos { get; set; } = true;
}

public enum EstadoCargaPreferencias {
    Exitosa,
    ArchivoInexistente,
    ArchivoVacio,
    ContenidoInvalido,
    VersionNoCompatible,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoCargaPreferencias {
    public EstadoCargaPreferencias Estado { get; init; }

    public PreferenciasAplicacion Preferencias { get; init; } = new();

    public Exception? Error { get; init; }

    public bool EsRecuperable => Estado is
        EstadoCargaPreferencias.Exitosa or
        EstadoCargaPreferencias.ArchivoInexistente or
        EstadoCargaPreferencias.ArchivoVacio;
}

public enum EstadoEscrituraPreferencias {
    Exitosa,
    ContenidoInvalido,
    VersionNoCompatible,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoEscrituraPreferencias {
    public EstadoEscrituraPreferencias Estado { get; init; }

    public Exception? Error { get; init; }

    public bool EsExitosa => Estado == EstadoEscrituraPreferencias.Exitosa;
}
