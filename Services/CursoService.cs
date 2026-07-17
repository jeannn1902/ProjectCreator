using EndForge.Models;

namespace EndForge.Services;

public sealed class CursoService {
    public const int TotalPracticasPlaneadas = 50;
    public const int PracticasPlaneadasPorTema = 5;

    private const string MensajeProximamente = "Contenido próximamente";
    private readonly IReadOnlyList<TemaCurso> temas;

    public CursoService() {
        temas = CrearTemas();
    }

    public IReadOnlyList<TemaCurso> CargarTemas() {
        return temas;
    }

    public TemaCurso? ObtenerTema(string temaId) {
        if (string.IsNullOrWhiteSpace(temaId)) {
            return null;
        }

        return temas.FirstOrDefault(tema =>
            tema.Id.Equals(temaId, StringComparison.OrdinalIgnoreCase));
    }

    public PracticaCurso? ObtenerPractica(string practicaId) {
        if (string.IsNullOrWhiteSpace(practicaId)) {
            return null;
        }

        return temas
            .SelectMany(tema => tema.Practicas)
            .FirstOrDefault(practica =>
                practica.Id.Equals(practicaId, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<TemaCurso> CrearTemas() {
        IReadOnlyList<PracticaCurso> practicasVariables = CrearPracticasVariables();

        return Array.AsReadOnly(new[] {
            CrearTemaDisponible(
                "variables",
                1,
                "Variables",
                "Aprende a guardar, modificar y utilizar información en tus programas.",
                practicasVariables),
            CrearTemaProximamente(
                "condicionales",
                2,
                "Condicionales",
                "Toma decisiones en tus programas según distintas condiciones."),
            CrearTemaProximamente(
                "ciclos",
                3,
                "Ciclos",
                "Repite instrucciones de forma controlada y eficiente."),
            CrearTemaProximamente(
                "funciones",
                4,
                "Funciones",
                "Organiza y reutiliza operaciones mediante funciones."),
            CrearTemaProximamente(
                "strings",
                5,
                "Strings",
                "Trabaja con texto, caracteres y operaciones sobre cadenas."),
            CrearTemaProximamente(
                "arrays",
                6,
                "Arrays",
                "Agrupa y procesa colecciones de datos de tamaño fijo."),
            CrearTemaProximamente(
                "structs",
                7,
                "Structs",
                "Modela información relacionada mediante estructuras."),
            CrearTemaProximamente(
                "vectores",
                8,
                "Vectores",
                "Administra colecciones dinámicas con la biblioteca estándar."),
            CrearTemaProximamente(
                "archivos",
                9,
                "Archivos",
                "Guarda y recupera información utilizando archivos."),
            CrearTemaProximamente(
                "poo",
                10,
                "POO",
                "Construye programas mediante clases, objetos y encapsulamiento.")
        });
    }

    private static TemaCurso CrearTemaDisponible(
        string id,
        int numero,
        string nombre,
        string descripcion,
        IReadOnlyList<PracticaCurso> practicas) {
        return new TemaCurso {
            Id = id,
            Numero = numero,
            Nombre = nombre,
            NombreCarpeta = $"{numero:00}_{nombre}",
            Descripcion = descripcion,
            TotalPracticasPlaneadas = PracticasPlaneadasPorTema,
            Practicas = practicas,
            EsProximamente = false
        };
    }

    private static TemaCurso CrearTemaProximamente(
        string id,
        int numero,
        string nombre,
        string descripcion) {
        return new TemaCurso {
            Id = id,
            Numero = numero,
            Nombre = nombre,
            NombreCarpeta = $"{numero:00}_{nombre}",
            Descripcion = descripcion,
            TotalPracticasPlaneadas = PracticasPlaneadasPorTema,
            EsProximamente = true,
            MensajeDisponibilidad = MensajeProximamente
        };
    }

    private static IReadOnlyList<PracticaCurso> CrearPracticasVariables() {
        return Array.AsReadOnly(new[] {
            new PracticaCurso {
                Id = "variables-datos-personales",
                TemaId = "variables",
                Numero = 1,
                Nombre = "Datos personales",
                NombreProyecto = "Datos personales",
                Objetivo = "Declarar variables de distintos tipos y mostrar su contenido.",
                Descripcion = "Crear un programa que almacene nombre, edad, estatura y estado de estudiante, y después muestre toda la información ordenada.",
                Conceptos = Array.AsReadOnly(new[] { "int", "double", "string", "bool", "cout", "cin" }),
                Instrucciones = Array.AsReadOnly(new[] {
                    "Declara variables para guardar el nombre, la edad, la estatura y el estado de estudiante.",
                    "Solicita al usuario los valores necesarios desde la consola.",
                    "Muestra todos los datos con etiquetas claras y en un orden fácil de leer."
                }),
                ResultadoEsperado = "La consola solicita los datos personales y después presenta un resumen ordenado con todos los valores.",
                Dificultad = "Inicial"
            },
            new PracticaCurso {
                Id = "variables-ticket-compra",
                TemaId = "variables",
                Numero = 2,
                Nombre = "Ticket de compra",
                NombreProyecto = "Ticket de compra",
                Objetivo = "Guardar precio y cantidad para calcular subtotal y total.",
                Descripcion = "Crear un programa que solicite el nombre de un producto, su precio y la cantidad comprada, y muestre un ticket sencillo.",
                Conceptos = Array.AsReadOnly(new[] { "string", "double", "int", "cout", "cin", "operadores aritméticos" }),
                Instrucciones = Array.AsReadOnly(new[] {
                    "Solicita el nombre, el precio unitario y la cantidad del producto.",
                    "Calcula el subtotal multiplicando el precio por la cantidad.",
                    "Presenta un ticket con los datos capturados y el total calculado."
                }),
                ResultadoEsperado = "La consola muestra un ticket legible con producto, precio, cantidad, subtotal y total.",
                Dificultad = "Inicial"
            },
            new PracticaCurso {
                Id = "variables-conversor-temperatura",
                TemaId = "variables",
                Numero = 3,
                Nombre = "Conversor de temperatura",
                NombreProyecto = "Conversor de temperatura",
                Objetivo = "Aplicar fórmulas usando variables decimales.",
                Descripcion = "Crear un programa que convierta una temperatura de grados Celsius a Fahrenheit.",
                Conceptos = Array.AsReadOnly(new[] { "double", "cout", "cin", "operadores aritméticos" }),
                Instrucciones = Array.AsReadOnly(new[] {
                    "Solicita una temperatura expresada en grados Celsius.",
                    "Aplica la fórmula de conversión utilizando variables decimales.",
                    "Muestra en consola el valor original y su equivalente en Fahrenheit."
                }),
                ResultadoEsperado = "La consola convierte correctamente una temperatura en Celsius y muestra el resultado en Fahrenheit.",
                Dificultad = "Inicial"
            },
            new PracticaCurso {
                Id = "variables-promedio-calificaciones",
                TemaId = "variables",
                Numero = 4,
                Nombre = "Promedio de calificaciones",
                NombreProyecto = "Promedio de calificaciones",
                Objetivo = "Guardar varias calificaciones y calcular su promedio.",
                Descripcion = "Crear un programa que solicite varias calificaciones, calcule el promedio y muestre el resultado.",
                Conceptos = Array.AsReadOnly(new[] { "double", "cout", "cin", "suma", "división" }),
                Instrucciones = Array.AsReadOnly(new[] {
                    "Declara una variable para cada calificación solicitada.",
                    "Suma las calificaciones y divide el resultado entre la cantidad de valores.",
                    "Muestra el promedio con una etiqueta clara."
                }),
                ResultadoEsperado = "La consola captura las calificaciones y muestra el promedio calculado con valores decimales.",
                Dificultad = "Inicial"
            },
            new PracticaCurso {
                Id = "variables-mini-recibo",
                TemaId = "variables",
                Numero = 5,
                Nombre = "Mini recibo",
                NombreProyecto = "Mini recibo",
                Objetivo = "Combinar texto, números y operaciones.",
                Descripcion = "Crear un recibo que incluya cliente, productos, cantidades, precios, subtotal y total.",
                Conceptos = Array.AsReadOnly(new[] { "string", "int", "double", "cout", "cin", "operadores aritméticos" }),
                Instrucciones = Array.AsReadOnly(new[] {
                    "Solicita el nombre del cliente y los datos de los productos.",
                    "Calcula los importes a partir de cantidades y precios.",
                    "Muestra un recibo ordenado con subtotales y total general."
                }),
                ResultadoEsperado = "La consola presenta un recibo completo y legible con cliente, productos, subtotales y total.",
                Dificultad = "Inicial"
            }
        });
    }
}
