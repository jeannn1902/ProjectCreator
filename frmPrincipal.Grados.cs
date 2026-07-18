using EndForge.Controls;
using EndForge.Models;
using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal {
    private enum VistaRutaAprendizaje {
        Ninguna,
        Grados,
        DetalleGrado,
        PracticasTema,
        DetallePractica,
        Evaluacion,
        Resultado,
        Historial
    }

    private GradosService gradosService = null!;
    private VistaRutaAprendizaje vistaRutaActual;
    private Panel panelGradosVista = null!;
    private PanelDesplazableSinBarras desplazamientoGrados = null!;
    private FlowLayoutPanel contenidoGrados = null!;
    private Button btnVolverGradosCurso = null!;
    private int ultimoAnchoContenidoGrados = -1;
    private int ultimoDpiContenidoGrados = -1;

    private void ConstruirVistaGrados() {
        desplazamientoGrados = CrearPanelDesplazableCurso(
            "desplazamientoGrados",
            "contenidoGrados",
            Point.Empty,
            panelGradosVista.ClientSize,
            new Padding(22, 18, 22, 24));
        desplazamientoGrados.Dock = DockStyle.None;
        desplazamientoGrados.BackColor = Color.FromArgb(18, 14, 27);
        desplazamientoGrados.Padding = Padding.Empty;
        desplazamientoGrados.MostrarBordeFoco = false;
        desplazamientoGrados.TabIndex = 0;
        contenidoGrados = desplazamientoGrados.Contenido;
        panelGradosVista.Controls.Add(desplazamientoGrados);
    }

    private void MostrarGrados() {
        if (!cursoPreparado) {
            return;
        }

        MostrarRutaAprendizajeInmersiva(reconstruirContenido: true);
    }

    private void AbrirGrado(GradoCurso grado) {
        if (!grado.EsContenidoDisponible) {
            return;
        }

        gradoSeleccionadoEnSesion = true;
        MostrarCursoPrincipal();
    }

    private void ReconstruirVistaGrados(bool volverAlInicio) {
        if (!cursoPreparado || contenidoGrados is null || contenidoGrados.IsDisposed) {
            return;
        }

        int anchoAreaDisponible = Math.Max(
            1,
            contenidoGrados.ClientSize.Width - contenidoGrados.Padding.Horizontal);
        bool usarDistribucionAmplia =
            rutaAprendizajeInmersivaActiva &&
            WindowState == FormWindowState.Maximized;
        int anchoContenido = Math.Min(
            EscalarDiseno(usarDistribucionAmplia ? 1500 : 1120),
            anchoAreaDisponible);
        int margenIzquierdo = usarDistribucionAmplia
            ? 0
            : Math.Max(0, (anchoAreaDisponible - anchoContenido) / 2);
        ultimoAnchoContenidoGrados = anchoAreaDisponible;
        ultimoDpiContenidoGrados = DeviceDpi;
        IReadOnlyList<GradoCurso> grados = gradosService.CargarGrados(progresoCurso);

        contenidoGrados.SuspendLayout();

        try {
            VaciarYDisponerControles(contenidoGrados);
            Label titulo = CrearLabelCurso(
                "Ruta de aprendizaje",
                Point.Empty,
                new Size(anchoContenido, EscalarDiseno(42)),
                TamanoFuenteCurso.TituloPrincipal,
                FontStyle.Bold,
                Color.White);
            titulo.Margin = new Padding(margenIzquierdo, 0, 0, EscalarDiseno(2));
            contenidoGrados.Controls.Add(titulo);

            Label subtitulo = CrearLabelCurso(
                "Avanza a tu propio ritmo y construye tus habilidades paso a paso.",
                Point.Empty,
                new Size(anchoContenido, EscalarDiseno(38)),
                TamanoFuenteCurso.DescripcionCabecera,
                FontStyle.Regular,
                ColorTextoSecundarioCurso);
            subtitulo.Margin = new Padding(margenIzquierdo, 0, 0, EscalarDiseno(14));
            contenidoGrados.Controls.Add(subtitulo);

            foreach (GradoCurso grado in grados.OrderBy(item => item.Numero)) {
                Panel tarjeta = CrearTarjetaGrado(grado, anchoContenido);
                tarjeta.Margin = new Padding(
                    margenIzquierdo,
                    0,
                    0,
                    EscalarDiseno(14));
                contenidoGrados.Controls.Add(tarjeta);
            }
        } finally {
            contenidoGrados.ResumeLayout(performLayout: true);
            desplazamientoGrados.ActualizarContenido(volverAlInicio);
        }
    }

    private Panel CrearTarjetaGrado(GradoCurso grado, int ancho) {
        bool disponible = grado.EsContenidoDisponible;
        int margen = EscalarDiseno(20);
        bool apilarAccion = disponible && ancho < EscalarDiseno(560);
        int alto = disponible
            ? apilarAccion
                ? EscalarDiseno(286)
                : EscalarDiseno(238)
            : EscalarDiseno(152);
        Panel tarjeta = CrearTarjetaCurso(
            Point.Empty,
            new Size(ancho, alto),
            16,
            interactiva: disponible);
        tarjeta.Margin = new Padding(0, 0, 0, EscalarDiseno(14));
        tarjeta.TabIndex = Math.Max(0, grado.Numero - 1);
        tarjeta.TabStop = disponible;
        tarjeta.AccessibleName = grado.Titulo;
        tarjeta.AccessibleDescription = disponible
            ? $"{ObtenerTextoEstadoGrado(grado.Estado)}. " +
              $"{grado.CantidadPracticasCompletadas} de {grado.CantidadPracticasDisponibles} prácticas disponibles realizadas. " +
              $"{grado.CantidadPracticasDisponibles} de {grado.CantidadPracticasPlaneadas} prácticas publicadas. " +
              $"{grado.Porcentaje} por ciento de progreso disponible."
            : "Contenido próximamente.";

        Label numero = CrearLabelCurso(
            $"GRADO {grado.Numero:00}",
            new Point(margen, EscalarDiseno(15)),
            new Size(Math.Max(1, ancho - margen * 2), EscalarDiseno(22)),
            TamanoFuenteCurso.NumeroCabecera,
            FontStyle.Bold,
            disponible ? ColorMoradoClaroCurso : Color.FromArgb(139, 132, 151));
        Label nombre = CrearLabelCurso(
            grado.Nombre,
            new Point(margen, EscalarDiseno(38)),
            new Size(Math.Max(1, ancho - margen * 2), EscalarDiseno(34)),
            TamanoFuenteCurso.TituloSeccion,
            FontStyle.Bold,
            disponible ? Color.White : Color.FromArgb(185, 180, 193));
        Label descripcion = CrearLabelCurso(
            grado.Descripcion,
            new Point(margen, EscalarDiseno(73)),
            new Size(Math.Max(1, ancho - margen * 2), EscalarDiseno(44)),
            TamanoFuenteCurso.DescripcionCabecera,
            FontStyle.Regular,
            disponible ? ColorTextoSecundarioCurso : Color.FromArgb(145, 139, 153));
        Label estado = CrearLabelCurso(
            ObtenerTextoEstadoGrado(grado.Estado),
            new Point(margen, disponible ? EscalarDiseno(121) : EscalarDiseno(120)),
            new Size(Math.Max(1, ancho - margen * 2), EscalarDiseno(24)),
            TamanoFuenteCurso.MetadatoTarjetaTema,
            FontStyle.Bold,
            disponible ? ColorMoradoClaroCurso : Color.FromArgb(157, 148, 169));

        tarjeta.Controls.Add(numero);
        tarjeta.Controls.Add(nombre);
        tarjeta.Controls.Add(descripcion);
        tarjeta.Controls.Add(estado);

        if (disponible) {
            int anchoBoton = apilarAccion
                ? Math.Max(1, ancho - margen * 2)
                : EscalarDiseno(172);
            int anchoMetricas = apilarAccion
                ? Math.Max(1, ancho - margen * 2)
                : Math.Max(1, ancho - margen * 2 - anchoBoton - EscalarDiseno(16));
            Label resumen = CrearLabelCurso(
                $"{grado.Temas.Count(tema => !tema.EsProximamente && tema.Practicas.Count > 0)} temas disponibles\n" +
                $"{grado.CantidadPracticasDisponibles} de {grado.CantidadPracticasPlaneadas} prácticas publicadas",
                new Point(margen, EscalarDiseno(145)),
                new Size(anchoMetricas, EscalarDiseno(42)),
                TamanoFuenteCurso.MetadatoTarjetaTema,
                FontStyle.Regular,
                ColorTextoSecundarioCurso,
                ContentAlignment.TopLeft);
            Label porcentaje = CrearLabelCurso(
                $"{grado.Porcentaje}% de progreso disponible",
                new Point(margen, EscalarDiseno(187)),
                new Size(anchoMetricas, EscalarDiseno(22)),
                TamanoFuenteCurso.ProgresoTarjetaTema,
                FontStyle.Bold,
                ColorMoradoClaroCurso);
            int anchoBarra = anchoMetricas;
            Panel fondoProgreso = new() {
                BackColor = Color.FromArgb(55, 45, 70),
                Location = new Point(margen, EscalarDiseno(211)),
                Size = new Size(anchoBarra, EscalarDiseno(9))
            };
            Panel progreso = new() {
                BackColor = ColorMoradoCurso,
                Location = Point.Empty,
                Size = new Size(
                    (int)Math.Round(anchoBarra * grado.Porcentaje / 100D),
                    fondoProgreso.Height)
            };
            fondoProgreso.Controls.Add(progreso);
            Button boton = CrearBotonCurso(
                grado.Estado == EstadoGradoCurso.EnProgreso
                    ? "Continuar grado"
                    : "Ver grado",
                apilarAccion
                    ? new Point(margen, EscalarDiseno(235))
                    : new Point(
                        Math.Max(margen, ancho - margen - anchoBoton),
                        EscalarDiseno(169)),
                new Size(anchoBoton, EscalarDiseno(36)),
                ColorMoradoCurso);
            boton.TabStop = false;
            boton.AccessibleDescription = $"Abre {grado.Titulo}.";

            tarjeta.Controls.Add(resumen);
            tarjeta.Controls.Add(porcentaje);
            tarjeta.Controls.Add(fondoProgreso);
            tarjeta.Controls.Add(boton);
            ConfigurarInteraccionTarjeta(tarjeta, () => AbrirGrado(grado));
        }

        return tarjeta;
    }

    private static string ObtenerTextoEstadoGrado(EstadoGradoCurso estado) {
        return estado switch {
            EstadoGradoCurso.EnProgreso => "EN PROGRESO",
            EstadoGradoCurso.ContenidoDisponibleCompletado =>
                "CONTENIDO DISPONIBLE COMPLETADO",
            EstadoGradoCurso.Completado => "COMPLETADO",
            EstadoGradoCurso.Proximamente => "PRÓXIMAMENTE",
            _ => "DISPONIBLE"
        };
    }

    private IEnumerable<Panel> ObtenerVistasRutaAprendizaje() {
        if (!cursoInicializado) {
            yield break;
        }

        yield return panelGradosVista;
        yield return panelCursoVista;
        yield return panelPracticasTemaVista;
        yield return panelDetallePracticaVista;

        foreach (Panel vista in ObtenerVistasEvaluacionInicializadas()) {
            yield return vista;
        }
    }

    private VistaRutaAprendizaje ObtenerEstadoVista(Panel vista) {
        if (ReferenceEquals(vista, panelGradosVista)) {
            return VistaRutaAprendizaje.Grados;
        }

        if (ReferenceEquals(vista, panelCursoVista)) {
            return VistaRutaAprendizaje.DetalleGrado;
        }

        if (ReferenceEquals(vista, panelPracticasTemaVista)) {
            return VistaRutaAprendizaje.PracticasTema;
        }

        if (ReferenceEquals(vista, panelDetallePracticaVista)) {
            return VistaRutaAprendizaje.DetallePractica;
        }

        return ObtenerEstadoVistaEvaluacion(vista);
    }

    private bool NavegarAtrasRuta() {
        switch (vistaRutaActual) {
            case VistaRutaAprendizaje.DetalleGrado:
                MostrarGrados();
                return true;
            case VistaRutaAprendizaje.PracticasTema:
                MostrarCursoPrincipal();
                return true;
            case VistaRutaAprendizaje.DetallePractica:
                if (temaCursoSeleccionado is not null) {
                    MostrarPracticasTema(temaCursoSeleccionado);
                } else {
                    MostrarCursoPrincipal();
                }
                return true;
            case VistaRutaAprendizaje.Evaluacion:
            case VistaRutaAprendizaje.Resultado:
            case VistaRutaAprendizaje.Historial:
                return NavegarAtrasEvaluacion(vistaRutaActual);
            default:
                return false;
        }
    }

    private void RecalcularDistribucionGrados() {
        if (!cursoInicializado || desplazamientoGrados is null) {
            return;
        }

        int anchoVista = Math.Max(1, panelGradosVista.ClientSize.Width);
        int altoVista = Math.Max(1, panelGradosVista.ClientSize.Height);
        bool usarDistribucionAmplia =
            rutaAprendizajeInmersivaActiva &&
            WindowState == FormWindowState.Maximized;
        int margenHorizontal = usarDistribucionAmplia
            ? EscalarDiseno(24)
            : 0;
        bool reservarZonaDecorativa =
            !usarDistribucionAmplia &&
            anchoVista >= EscalarDiseno(1200);
        int anchoDesplazamiento = usarDistribucionAmplia
            ? Math.Min(
                EscalarDiseno(1500),
                Math.Max(1, anchoVista - margenHorizontal * 2))
            : reservarZonaDecorativa
                ? Math.Max(1, (int)Math.Round(anchoVista * 0.72D))
                : anchoVista;
        desplazamientoGrados.SetBounds(
            margenHorizontal,
            0,
            anchoDesplazamiento,
            altoVista);

        if (!panelGradosVista.Visible) {
            return;
        }

        int ancho = Math.Max(
            1,
            contenidoGrados.ClientSize.Width - contenidoGrados.Padding.Horizontal);

        if (Math.Abs(ancho - ultimoAnchoContenidoGrados) >= EscalarDiseno(8) ||
            ultimoDpiContenidoGrados != DeviceDpi) {
            ReconstruirVistaGrados(volverAlInicio: false);
        } else {
            desplazamientoGrados.ActualizarContenido(volverAlInicio: false);
        }
    }
}
