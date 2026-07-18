namespace EndForge.Models;

public sealed class SolicitudEvaluacionPractica {
    public string PracticaId { get; init; } = "";

    public string RutaProyecto { get; init; } = "";
}

public enum EtapaEvaluacionPractica {
    Preparando,
    Compilando,
    EjecutandoPruebas,
    GenerandoResultado
}

public sealed class ProgresoEvaluacionPractica {
    public EtapaEvaluacionPractica Etapa { get; init; }

    public string Mensaje { get; init; } = "";

    public int CasosCompletados { get; init; }

    public int CasosTotales { get; init; }
}

public enum EstadoProcesoEvaluacionPractica {
    Finalizada,
    PracticaNoEvaluable,
    ProyectoNoDisponible,
    EntornoNoDisponible,
    Cancelada,
    ErrorInfraestructura
}

public sealed class ResultadoProcesoEvaluacionPractica {
    public EstadoProcesoEvaluacionPractica Estado { get; init; }

    public ResultadoEvaluacion? Resultado { get; init; }

    public string Mensaje { get; init; } = "";

    public Exception? Error { get; init; }

    public bool EsIntentoCalificable =>
        Estado == EstadoProcesoEvaluacionPractica.Finalizada && Resultado is not null;
}
