using EndForge.Models;

namespace EndForge.Services;

public sealed class GradosService {
    public const string GradoFundamentosId = "grado-1-fundamentos-cpp";
    public const int MetaCurricularPracticasGradoFundamentos =
        CursoService.TotalPracticasPlaneadas;

    private readonly CursoService cursoService;

    public GradosService(CursoService cursoService) {
        this.cursoService = cursoService;
    }

    public IReadOnlyList<GradoCurso> CargarGrados(ProgresoCurso? progreso) {
        IReadOnlyList<TemaCurso> temas = cursoService.CargarTemas();
        HashSet<string> practicasDisponibles = temas
            .Where(tema => !tema.EsProximamente)
            .SelectMany(tema => tema.Practicas)
            .Select(practica => practica.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        int realizadas = progreso?.Practicas
            .Where(item =>
                item.Estado == EstadoPracticaCurso.Realizada &&
                practicasDisponibles.Contains(item.PracticaId))
            .Select(item => item.PracticaId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() ?? 0;
        bool existeProgreso = progreso?.Practicas.Any(item =>
            practicasDisponibles.Contains(item.PracticaId) &&
            (item.Estado != EstadoPracticaCurso.Pendiente ||
             !string.IsNullOrWhiteSpace(item.RutaProyecto))) == true;
        int disponibles = practicasDisponibles.Count;
        int porcentaje = disponibles == 0
            ? 0
            : Math.Clamp((int)Math.Round(realizadas * 100D / disponibles), 0, 100);
        EstadoGradoCurso estado = realizadas >= disponibles && disponibles > 0
            ? disponibles < MetaCurricularPracticasGradoFundamentos
                ? EstadoGradoCurso.ContenidoDisponibleCompletado
                : EstadoGradoCurso.Completado
            : existeProgreso
                ? EstadoGradoCurso.EnProgreso
                : EstadoGradoCurso.Disponible;

        return Array.AsReadOnly(new[] {
            new GradoCurso {
                Id = GradoFundamentosId,
                Numero = 1,
                Nombre = "Fundamentos de C++",
                Descripcion = "Aprende las bases esenciales de la programación en C++, desde variables y decisiones hasta archivos e introducción a la programación orientada a objetos.",
                Estado = estado,
                Temas = temas,
                Porcentaje = porcentaje,
                CantidadPracticasDisponibles = disponibles,
                CantidadPracticasCompletadas = realizadas,
                CantidadPracticasPlaneadas = MetaCurricularPracticasGradoFundamentos,
                EsContenidoDisponible = true
            },
            CrearProximamente(2, "C++ Junior", "Consolida las bases y comienza a construir programas más completos."),
            CrearProximamente(3, "C++ Intermedio", "Profundiza en diseño, memoria, estructuras y herramientas del lenguaje."),
            CrearProximamente(4, "Desarrollo avanzado", "Explora técnicas avanzadas para proyectos de mayor escala."),
            CrearProximamente(5, "Especializaciones", "Elige rutas especializadas y aplica C++ en contextos profesionales.")
        });
    }

    public GradoCurso? ObtenerGrado(string gradoId, ProgresoCurso? progreso) {
        if (string.IsNullOrWhiteSpace(gradoId)) {
            return null;
        }

        return CargarGrados(progreso).FirstOrDefault(grado =>
            grado.Id.Equals(gradoId, StringComparison.OrdinalIgnoreCase));
    }

    private static GradoCurso CrearProximamente(
        int numero,
        string nombre,
        string descripcion) {
        return new GradoCurso {
            Id = $"grado-{numero}",
            Numero = numero,
            Nombre = nombre,
            Descripcion = descripcion,
            Estado = EstadoGradoCurso.Proximamente,
            EsContenidoDisponible = false
        };
    }
}
