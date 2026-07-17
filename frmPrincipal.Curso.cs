using EndForge.Models;
using EndForge.Services;
using EndForge.Controls;
using System.Drawing.Drawing2D;

namespace EndForge;

public partial class frmPrincipal {
    private static readonly Color ColorTarjetaCurso = Color.FromArgb(24, 19, 36);
    private static readonly Color ColorTarjetaCursoHover = Color.FromArgb(41, 31, 58);
    private static readonly Color ColorBordeCurso = Color.FromArgb(116, 67, 163);
    private static readonly Color ColorMoradoCurso = Color.FromArgb(145, 82, 214);
    private static readonly Color ColorMoradoClaroCurso = Color.FromArgb(202, 151, 247);
    private static readonly Color ColorTextoSecundarioCurso = Color.FromArgb(190, 181, 204);

    private readonly CursoService cursoService = new();
    private readonly ProgresoCursoService progresoCursoService = new();
    private ProgresoCurso progresoCurso = new();
    private TemaCurso? temaCursoSeleccionado;
    private PracticaCurso? practicaCursoSeleccionada;
    private string mensajeEstadoProgresoCurso = string.Empty;

    private Panel panelCurso = null!;
    private Label lblCurso = null!;
    private PictureBox pictureBoxCurso = null!;
    private Panel panelIndicadorCurso = null!;
    private Panel panelCursoVista = null!;
    private Panel panelPracticasTemaVista = null!;
    private Panel panelDetallePracticaVista = null!;
    private PanelDesplazableSinBarras desplazamientoTemasCurso = null!;
    private PanelDesplazableSinBarras desplazamientoPracticasTema = null!;
    private PanelDesplazableSinBarras desplazamientoDetallePractica = null!;
    private Panel panelPerfilCurso = null!;
    private Label lblTituloTemasCurso = null!;
    private FlowLayoutPanel listaTemasCurso = null!;
    private FlowLayoutPanel listaPracticasTema = null!;
    private FlowLayoutPanel contenidoDetallePractica = null!;
    private Label lblProgresoGeneralCurso = null!;
    private Label lblUltimaPracticaCurso = null!;
    private Label lblAvisoProgresoCurso = null!;
    private Panel panelRellenoProgresoCurso = null!;
    private Button btnContinuarAprendizaje = null!;
    private Label lblNumeroTemaCurso = null!;
    private Label lblNombreTemaCurso = null!;
    private Label lblDescripcionTemaCurso = null!;
    private Size tamanoPanelPrincipalNormal;
    private bool modoCursoInmersivo;

    private void InicializarCurso() {
        tamanoPanelPrincipalNormal = panelPrincipal.Size;
        InicializarOpcionCursoMenu();
        InicializarVistasCurso();
        ConstruirResumenCurso();
        ConstruirVistaPracticasTema();
        ConstruirVistaDetallePractica();

        ActivarDobleBuffer(panelCursoVista);
        ActivarDobleBuffer(panelPracticasTemaVista);
        ActivarDobleBuffer(panelDetallePracticaVista);
        ActivarDobleBuffer(desplazamientoTemasCurso);
        ActivarDobleBuffer(desplazamientoPracticasTema);
        ActivarDobleBuffer(desplazamientoDetallePractica);
    }

    private void InicializarOpcionCursoMenu() {
        panelAbrirPractica.Location = new Point(12, 338);
        panelRecientes.Location = new Point(12, 394);
        panelConfiguracion.Location = new Point(12, 450);
        panelAcercaDe.Location = new Point(12, 506);

        panelCurso = new Panel {
            Name = "panelCurso",
            BackColor = Color.FromArgb(45, 45, 48),
            Cursor = Cursors.Hand,
            Location = new Point(12, 282),
            Size = new Size(195, 48),
            TabIndex = 5
        };

        panelIndicadorCurso = new Panel {
            Name = "panelIndicadorCurso",
            BackColor = ColorMoradoCurso,
            Location = Point.Empty,
            Size = new Size(3, 48),
            TabIndex = 2,
            Visible = false
        };

        pictureBoxCurso = new PictureBox {
            Name = "pictureBoxCurso",
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Location = new Point(15, 13),
            Size = new Size(24, 24),
            SizeMode = PictureBoxSizeMode.Zoom,
            TabIndex = 0,
            TabStop = false
        };

        lblCurso = new Label {
            Name = "lblCurso",
            AutoSize = false,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Semibold", 10F),
            ForeColor = Color.White,
            Location = new Point(49, 10),
            Size = new Size(146, 28),
            TabIndex = 1,
            Text = "Curso",
            TextAlign = ContentAlignment.MiddleLeft
        };

        panelCurso.Controls.Add(panelIndicadorCurso);
        panelCurso.Controls.Add(lblCurso);
        panelCurso.Controls.Add(pictureBoxCurso);
        panelMenu.Controls.Add(panelCurso);

        panelCurso.MouseEnter += PanelMenu_MouseEnter;
        panelCurso.MouseLeave += PanelMenu_MouseLeave;
        lblCurso.MouseEnter += PanelMenu_MouseEnter;
        lblCurso.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxCurso.MouseEnter += PanelMenu_MouseEnter;
        pictureBoxCurso.MouseLeave += PanelMenu_MouseLeave;
        panelCurso.Click += PanelCurso_Click;
        lblCurso.Click += PanelCurso_Click;
        pictureBoxCurso.Click += PanelCurso_Click;
    }

    private void InicializarVistasCurso() {
        panelCursoVista = CrearVistaCurso("panelCursoVista");
        panelPracticasTemaVista = CrearVistaCurso("panelPracticasTemaVista");
        panelDetallePracticaVista = CrearVistaCurso("panelDetallePracticaVista");

        panelPrincipal.Controls.Add(panelCursoVista);
        panelPrincipal.Controls.Add(panelPracticasTemaVista);
        panelPrincipal.Controls.Add(panelDetallePracticaVista);

        panelCursoVista.Visible = false;
        panelPracticasTemaVista.Visible = false;
        panelDetallePracticaVista.Visible = false;
    }

    private static Panel CrearVistaCurso(string nombre) {
        return new Panel {
            Name = nombre,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Location = Point.Empty,
            Size = new Size(892, 470)
        };
    }

    private static PanelDesplazableSinBarras CrearPanelDesplazableCurso(
        string nombre,
        string nombreContenido,
        Point ubicacion,
        Size tamano,
        Padding rellenoContenido) {
        PanelDesplazableSinBarras desplazamiento = new() {
            Name = nombre,
            BackColor = ColorBordeCurso,
            ColorFondoContenido = Color.FromArgb(18, 14, 27),
            Location = ubicacion,
            Size = tamano
        };
        desplazamiento.Contenido.Name = nombreContenido;
        desplazamiento.Contenido.Padding = rellenoContenido;
        return desplazamiento;
    }

    private void ConstruirResumenCurso() {
        panelPerfilCurso = CrearTarjetaCurso(new Point(18, 18), new Size(278, 434), 18);

        Label lblTituloPerfil = CrearLabelCurso(
            "Curso de C++",
            new Point(22, 24),
            new Size(232, 38),
            15F,
            FontStyle.Bold,
            Color.White);
        Label lblNombrePerfil = CrearLabelCurso(
            "Jean Pérez",
            new Point(22, 72),
            new Size(232, 27),
            11F,
            FontStyle.Bold,
            ColorMoradoClaroCurso);
        Label lblNivelPerfil = CrearLabelCurso(
            "Nivel actual: Pre-Junior",
            new Point(22, 106),
            new Size(232, 24),
            9.5F,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);

        lblProgresoGeneralCurso = CrearLabelCurso(
            "Progreso general\n0 de 50 prácticas",
            new Point(22, 140),
            new Size(232, 52),
            8.8F,
            FontStyle.Bold,
            Color.White,
            ContentAlignment.TopLeft);

        Panel fondoBarra = new() {
            BackColor = Color.FromArgb(55, 45, 70),
            Location = new Point(22, 199),
            Size = new Size(232, 11)
        };
        panelRellenoProgresoCurso = new Panel {
            BackColor = ColorMoradoCurso,
            Location = Point.Empty,
            Size = new Size(0, 11)
        };
        fondoBarra.Controls.Add(panelRellenoProgresoCurso);

        Label lblSeparador = CrearLabelCurso(
            "TU SIGUIENTE PASO",
            new Point(22, 228),
            new Size(232, 22),
            8F,
            FontStyle.Bold,
            Color.FromArgb(157, 135, 181));

        lblUltimaPracticaCurso = CrearLabelCurso(
            "Aún no hay prácticas iniciadas.",
            new Point(22, 259),
            new Size(232, 57),
            9F,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);

        btnContinuarAprendizaje = CrearBotonCurso(
            "Continuar aprendizaje",
            new Point(22, 323),
            new Size(232, 40),
            ColorMoradoCurso);
        btnContinuarAprendizaje.Visible = false;
        btnContinuarAprendizaje.Click += BtnContinuarAprendizaje_Click;

        lblAvisoProgresoCurso = CrearLabelCurso(
            string.Empty,
            new Point(22, 373),
            new Size(232, 44),
            8F,
            FontStyle.Regular,
            Color.FromArgb(235, 181, 89));
        lblAvisoProgresoCurso.Visible = false;

        panelPerfilCurso.Controls.Add(lblTituloPerfil);
        panelPerfilCurso.Controls.Add(lblNombrePerfil);
        panelPerfilCurso.Controls.Add(lblNivelPerfil);
        panelPerfilCurso.Controls.Add(lblProgresoGeneralCurso);
        panelPerfilCurso.Controls.Add(fondoBarra);
        panelPerfilCurso.Controls.Add(lblSeparador);
        panelPerfilCurso.Controls.Add(lblUltimaPracticaCurso);
        panelPerfilCurso.Controls.Add(btnContinuarAprendizaje);
        panelPerfilCurso.Controls.Add(lblAvisoProgresoCurso);

        lblTituloTemasCurso = CrearLabelCurso(
            "TEMAS DEL CURSO",
            new Point(320, 20),
            new Size(554, 30),
            14F,
            FontStyle.Bold,
            Color.White);

        desplazamientoTemasCurso = CrearPanelDesplazableCurso(
            "desplazamientoTemasCurso",
            "listaTemasCurso",
            new Point(316, 56),
            new Size(560, 396),
            new Padding(4, 3, 4, 4));
        listaTemasCurso = desplazamientoTemasCurso.Contenido;

        panelCursoVista.Controls.Add(panelPerfilCurso);
        panelCursoVista.Controls.Add(lblTituloTemasCurso);
        panelCursoVista.Controls.Add(desplazamientoTemasCurso);
    }

    private void ConstruirVistaPracticasTema() {
        Button btnVolverTemas = CrearBotonSecundarioCurso(
            "← Volver a temas",
            new Point(18, 14),
            new Size(182, 32));
        btnVolverTemas.Click += (_, _) => MostrarCursoPrincipal();

        lblNumeroTemaCurso = CrearLabelCurso(
            "Tema 01",
            new Point(20, 58),
            new Size(150, 24),
            9F,
            FontStyle.Bold,
            ColorMoradoClaroCurso);
        lblNombreTemaCurso = CrearLabelCurso(
            "Variables",
            new Point(20, 79),
            new Size(850, 47),
            21F,
            FontStyle.Bold,
            Color.White);
        lblDescripcionTemaCurso = CrearLabelCurso(
            string.Empty,
            new Point(20, 125),
            new Size(850, 34),
            9.5F,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);

        desplazamientoPracticasTema = CrearPanelDesplazableCurso(
            "desplazamientoPracticasTema",
            "listaPracticasTema",
            new Point(16, 163),
            new Size(860, 289),
            new Padding(4, 2, 4, 4));
        listaPracticasTema = desplazamientoPracticasTema.Contenido;

        panelPracticasTemaVista.Controls.Add(btnVolverTemas);
        panelPracticasTemaVista.Controls.Add(lblNumeroTemaCurso);
        panelPracticasTemaVista.Controls.Add(lblNombreTemaCurso);
        panelPracticasTemaVista.Controls.Add(lblDescripcionTemaCurso);
        panelPracticasTemaVista.Controls.Add(desplazamientoPracticasTema);
    }

    private void ConstruirVistaDetallePractica() {
        Button btnVolverPracticas = CrearBotonSecundarioCurso(
            "← Volver a prácticas",
            new Point(18, 14),
            new Size(202, 32));
        btnVolverPracticas.Click += (_, _) => {
            if (temaCursoSeleccionado is not null) {
                MostrarPracticasTema(temaCursoSeleccionado);
            } else {
                MostrarCursoPrincipal();
            }
        };

        desplazamientoDetallePractica = CrearPanelDesplazableCurso(
            "desplazamientoDetallePractica",
            "contenidoDetallePractica",
            new Point(16, 54),
            new Size(860, 398),
            new Padding(12, 10, 12, 18));
        contenidoDetallePractica = desplazamientoDetallePractica.Contenido;

        panelDetallePracticaVista.Controls.Add(btnVolverPracticas);
        panelDetallePracticaVista.Controls.Add(desplazamientoDetallePractica);
    }

    private void PanelCurso_Click(object? sender, EventArgs e) {
        SeleccionarPanelMenu(panelCurso);

        panelInicioVista.Visible = false;
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = false;

        CargarProgresoCurso(mostrarAvisoInmediato: false);
        MostrarCursoPrincipal();
    }

    private void MostrarCursoPrincipal() {
        MostrarNavegacionPrincipal();
        ActualizarResumenCurso();
        ReconstruirTarjetasTemas();
        MostrarSubvistaCurso(panelCursoVista);
    }

    private void MostrarPracticasTema(TemaCurso tema) {
        if (tema.EsProximamente) {
            MessageBox.Show(
                tema.MensajeDisponibilidad,
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        temaCursoSeleccionado = tema;
        MostrarModoCursoInmersivo();
        lblNumeroTemaCurso.Text = $"Tema {tema.Numero:00}";
        lblNombreTemaCurso.Text = tema.Nombre;
        lblDescripcionTemaCurso.Text = tema.Descripcion;
        ReconstruirTarjetasPracticas(tema);
        MostrarSubvistaCurso(panelPracticasTemaVista);
    }

    private void MostrarDetallePractica(PracticaCurso practica) {
        practicaCursoSeleccionada = practica;
        temaCursoSeleccionado = cursoService.ObtenerTema(practica.TemaId);
        MostrarModoCursoInmersivo();
        ReconstruirDetallePractica(practica);
        MostrarSubvistaCurso(panelDetallePracticaVista);
    }

    private void MostrarNavegacionPrincipal() {
        timerRecalcularVista.Stop();
        modoCursoInmersivo = false;
        panelMenu.Visible = true;
        panelPrincipal.Visible = true;

        CentrarPanelPrincipal();
        SincronizarVistaPrincipalCurso();
        AjustarGeometriaVistasCurso();

        panelMenu.BringToFront();
        panelBarraTitulo.BringToFront();
        InvalidarFondoContinuo();
    }

    private void SincronizarVistaPrincipalCurso() {
        Rectangle areaDisponible = panelPrincipal.ClientRectangle;

        panelCursoVista.SetBounds(
            areaDisponible.Left,
            areaDisponible.Top,
            Math.Max(1, areaDisponible.Width),
            Math.Max(1, areaDisponible.Height));
    }

    private void MostrarModoCursoInmersivo() {
        timerRecalcularVista.Stop();
        modoCursoInmersivo = true;
        panelMenu.Visible = false;
        panelPrincipal.Visible = true;
        panelBarraTitulo.BringToFront();
        CentrarPanelPrincipal();
        AjustarGeometriaVistasCurso();
        InvalidarFondoContinuo();
    }

    private void MostrarSubvistaCurso(Panel vista) {
        panelCursoVista.Visible = ReferenceEquals(vista, panelCursoVista);
        panelPracticasTemaVista.Visible = ReferenceEquals(vista, panelPracticasTemaVista);
        panelDetallePracticaVista.Visible = ReferenceEquals(vista, panelDetallePracticaVista);
        vista.BringToFront();
    }

    private void OcultarVistasCurso() {
        panelCursoVista.Visible = false;
        panelPracticasTemaVista.Visible = false;
        panelDetallePracticaVista.Visible = false;
    }

    private void AjustarGeometriaVistasCurso() {
        if (desplazamientoTemasCurso is null ||
            desplazamientoPracticasTema is null ||
            desplazamientoDetallePractica is null ||
            panelPerfilCurso is null ||
            lblTituloTemasCurso is null) {
            return;
        }

        int ancho = Math.Max(1, panelPrincipal.ClientSize.Width);
        int alto = Math.Max(1, panelPrincipal.ClientSize.Height);
        int anchoVistaCurso = Math.Max(1, panelCursoVista.ClientSize.Width);
        int altoVistaCurso = Math.Max(1, panelCursoVista.ClientSize.Height);
        const int margenHorizontalCurso = 16;
        const int margenVerticalCurso = 12;
        const int anchoPerfilPreferido = 278;
        const int anchoTemasPreferido = 560;
        const int separacionColumnas = 20;
        const int altoBloquePreferido = 434;
        const int altoTituloTemas = 30;
        const int separacionTituloLista = 8;

        int anchoBloqueDisponible = Math.Max(1, anchoVistaCurso - margenHorizontalCurso * 2);
        int anchoBloque = Math.Min(
            anchoPerfilPreferido + separacionColumnas + anchoTemasPreferido,
            anchoBloqueDisponible);
        int separacionReal = Math.Min(separacionColumnas, Math.Max(0, anchoBloque - 2));
        int anchoPerfil = Math.Min(
            anchoPerfilPreferido,
            Math.Max(1, anchoBloque - separacionReal - 1));
        int anchoTemas = Math.Max(1, anchoBloque - anchoPerfil - separacionReal);
        int xBloque = Math.Max(0, (anchoVistaCurso - anchoBloque) / 2);
        int xTemas = xBloque + anchoPerfil + separacionReal;
        int altoBloque = Math.Min(
            altoBloquePreferido,
            Math.Max(1, altoVistaCurso - margenVerticalCurso * 2));
        int yBloque = Math.Max(0, (altoVistaCurso - altoBloque) / 2);
        int altoTituloReal = Math.Min(altoTituloTemas, altoBloque);
        int separacionTituloReal = Math.Min(
            separacionTituloLista,
            Math.Max(0, altoBloque - altoTituloReal - 1));
        int altoLista = Math.Max(1, altoBloque - altoTituloReal - separacionTituloReal);

        panelPerfilCurso.SetBounds(xBloque, yBloque, anchoPerfil, altoBloque);
        lblTituloTemasCurso.SetBounds(
            xTemas,
            yBloque,
            anchoTemas,
            altoTituloReal);
        desplazamientoTemasCurso.SetBounds(
            xTemas,
            yBloque + altoTituloReal + separacionTituloReal,
            anchoTemas,
            altoLista);

        lblNombreTemaCurso.Width = Math.Max(120, ancho - 40);
        lblDescripcionTemaCurso.Width = Math.Max(120, ancho - 40);
        desplazamientoPracticasTema.SetBounds(
            16,
            163,
            Math.Max(120, ancho - 32),
            Math.Max(120, alto - 181));

        desplazamientoDetallePractica.SetBounds(
            16,
            54,
            Math.Max(120, ancho - 32),
            Math.Max(120, alto - 72));

        desplazamientoTemasCurso.ActualizarContenido(volverAlInicio: false);
        desplazamientoPracticasTema.ActualizarContenido(volverAlInicio: false);
        desplazamientoDetallePractica.ActualizarContenido(volverAlInicio: false);
    }

    private void ActualizarDisenoCursoTrasRedimensionar() {
        AjustarGeometriaVistasCurso();

        if (panelDetallePracticaVista.Visible && practicaCursoSeleccionada is not null) {
            ReconstruirDetallePractica(practicaCursoSeleccionada, volverAlInicio: false);
        } else if (panelPracticasTemaVista.Visible && temaCursoSeleccionado is not null) {
            ReconstruirTarjetasPracticas(temaCursoSeleccionado, volverAlInicio: false);
        }
    }

    private void CargarProgresoCurso() {
        CargarProgresoCurso(mostrarAvisoInmediato: false);
    }

    private void CargarProgresoCurso(bool mostrarAvisoInmediato) {
        ResultadoCargaProgreso resultado = progresoCursoService.CargarProgreso();

        if (resultado.DatosDisponibles) {
            progresoCurso = resultado.Progreso;
        }

        mensajeEstadoProgresoCurso = resultado.Estado switch {
            EstadoCargaProgreso.ContenidoInvalido when resultado.Progreso.Practicas.Count > 0 =>
                $"Se ignoraron {resultado.RegistrosInvalidos} registros de progreso dañados.",
            EstadoCargaProgreso.ContenidoInvalido =>
                "No se pudo leer progreso.json porque su contenido está dañado.",
            EstadoCargaProgreso.PermisosInsuficientes =>
                "No hay permisos para leer progreso.json. Se muestra un progreso vacío seguro.",
            EstadoCargaProgreso.ErrorIo =>
                "No se pudo leer progreso.json porque está bloqueado o no está disponible.",
            _ => string.Empty
        };

        if (mostrarAvisoInmediato && !string.IsNullOrWhiteSpace(mensajeEstadoProgresoCurso)) {
            MessageBox.Show(
                mensajeEstadoProgresoCurso,
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        if (panelPerfilCurso is not null) {
            ActualizarResumenCurso();
        }
    }

    private void ActualizarResumenCurso() {
        int realizadas = progresoCurso.Practicas.Count(progreso =>
            progreso.Estado == EstadoPracticaCurso.Realizada &&
            cursoService.ObtenerPractica(progreso.PracticaId) is not null);

        lblProgresoGeneralCurso.Text =
            $"Progreso general\n{realizadas} de {CursoService.TotalPracticasPlaneadas} prácticas";

        int anchoDisponible = panelRellenoProgresoCurso.Parent?.ClientSize.Width ?? 232;
        int anchoBarra = (int)Math.Round(
            anchoDisponible * realizadas / (double)CursoService.TotalPracticasPlaneadas);
        panelRellenoProgresoCurso.Width = Math.Clamp(anchoBarra, 0, anchoDisponible);

        ProgresoPractica? siguiente = progresoCurso.Practicas
            .Where(progreso =>
                progreso.Estado != EstadoPracticaCurso.Realizada &&
                cursoService.ObtenerPractica(progreso.PracticaId) is not null &&
                !string.IsNullOrWhiteSpace(progreso.RutaProyecto) &&
                Directory.Exists(progreso.RutaProyecto))
            .OrderByDescending(progreso => progreso.FechaCreacion)
            .FirstOrDefault();

        if (siguiente is null) {
            lblUltimaPracticaCurso.Text = realizadas > 0
                ? $"Has completado {realizadas} práctica{(realizadas == 1 ? string.Empty : "s")}."
                : "Aún no hay prácticas iniciadas.";
            btnContinuarAprendizaje.Visible = false;
            btnContinuarAprendizaje.Tag = null;
        } else {
            PracticaCurso practica = cursoService.ObtenerPractica(siguiente.PracticaId)!;
            TemaCurso? tema = cursoService.ObtenerTema(practica.TemaId);
            lblUltimaPracticaCurso.Text =
                $"Última práctica: {practica.Nombre}\nTema actual: {tema?.Nombre ?? "Curso"}";
            btnContinuarAprendizaje.Visible = true;
            btnContinuarAprendizaje.Tag = practica;
        }

        lblAvisoProgresoCurso.Text = mensajeEstadoProgresoCurso;
        lblAvisoProgresoCurso.Visible = !string.IsNullOrWhiteSpace(mensajeEstadoProgresoCurso);
    }

    private void BtnContinuarAprendizaje_Click(object? sender, EventArgs e) {
        if (btnContinuarAprendizaje.Tag is PracticaCurso practica) {
            MostrarDetallePractica(practica);
        }
    }

    private void ReconstruirTarjetasTemas() {
        listaTemasCurso.SuspendLayout();

        try {
            VaciarYDisponerControles(listaTemasCurso);

        foreach (TemaCurso tema in cursoService.CargarTemas()) {
            int realizadas = ContarPracticasRealizadas(tema);
            EstadoPracticaCurso estadoTema = ObtenerEstadoTema(tema, realizadas);
            Panel tarjeta = CrearTarjetaCurso(Point.Empty, new Size(526, 112), 14);
            tarjeta.Margin = new Padding(0, 0, 0, 10);

            Label numero = CrearLabelCurso(
                tema.Numero.ToString("00"),
                new Point(18, 14),
                new Size(42, 26),
                12F,
                FontStyle.Bold,
                ColorMoradoClaroCurso);
            Label nombre = CrearLabelCurso(
                tema.Nombre,
                new Point(64, 11),
                new Size(260, 28),
                12.5F,
                FontStyle.Bold,
                Color.White);
            Label descripcion = CrearLabelCurso(
                PrepararDescripcionTarjeta(tema.Descripcion),
                new Point(64, 40),
                new Size(300, 65),
                8F,
                FontStyle.Regular,
                ColorTextoSecundarioCurso,
                ContentAlignment.TopLeft);
            Label progreso = CrearLabelCurso(
                $"{realizadas} de {tema.TotalPracticasPlaneadas} prácticas",
                new Point(368, 18),
                new Size(140, 21),
                8.5F,
                FontStyle.Bold,
                Color.White,
                ContentAlignment.MiddleRight);
            Label estado = CrearLabelCurso(
                ObtenerTextoEstadoTema(estadoTema),
                new Point(348, 48),
                new Size(160, 23),
                8.5F,
                FontStyle.Bold,
                ObtenerColorEstado(estadoTema),
                ContentAlignment.MiddleRight);

            tarjeta.Controls.Add(numero);
            tarjeta.Controls.Add(nombre);
            tarjeta.Controls.Add(descripcion);
            tarjeta.Controls.Add(progreso);
            tarjeta.Controls.Add(estado);
            ConfigurarInteraccionTarjeta(tarjeta, () => MostrarPracticasTema(tema));
            listaTemasCurso.Controls.Add(tarjeta);
            }
        } finally {
            listaTemasCurso.ResumeLayout(performLayout: true);
            desplazamientoTemasCurso.ActualizarContenido(volverAlInicio: true);
        }
    }

    private void ReconstruirTarjetasPracticas(TemaCurso tema, bool volverAlInicio = true) {
        listaPracticasTema.SuspendLayout();

        try {
            VaciarYDisponerControles(listaPracticasTema);
            int anchoTarjeta = Math.Max(
                420,
                listaPracticasTema.ClientSize.Width - listaPracticasTema.Padding.Horizontal - 4);
            int xEstado = Math.Max(210, anchoTarjeta - 202);
            int anchoTexto = Math.Max(120, xEstado - 84);

        foreach (PracticaCurso practica in tema.Practicas.OrderBy(item => item.Numero)) {
            EstadoPracticaCurso estadoPractica = ObtenerEstadoPractica(practica.Id);
            Panel tarjeta = CrearTarjetaCurso(Point.Empty, new Size(anchoTarjeta, 86), 14);
            tarjeta.Margin = new Padding(0, 0, 0, 10);

            Label numero = CrearLabelCurso(
                practica.Numero.ToString("00"),
                new Point(18, 14),
                new Size(45, 26),
                12F,
                FontStyle.Bold,
                ColorMoradoClaroCurso);
            Label nombre = CrearLabelCurso(
                practica.Nombre,
                new Point(68, 10),
                new Size(anchoTexto, 28),
                12F,
                FontStyle.Bold,
                Color.White);
            Label descripcion = CrearLabelCurso(
                practica.Objetivo,
                new Point(68, 39),
                new Size(anchoTexto, 35),
                8.8F,
                FontStyle.Regular,
                ColorTextoSecundarioCurso);
            Label estado = CrearLabelCurso(
                ObtenerTextoEstadoPractica(estadoPractica),
                new Point(xEstado, 27),
                new Size(180, 28),
                9F,
                FontStyle.Bold,
                ObtenerColorEstado(estadoPractica),
                ContentAlignment.MiddleRight);

            tarjeta.Controls.Add(numero);
            tarjeta.Controls.Add(nombre);
            tarjeta.Controls.Add(descripcion);
            tarjeta.Controls.Add(estado);
            ConfigurarInteraccionTarjeta(tarjeta, () => MostrarDetallePractica(practica));
            listaPracticasTema.Controls.Add(tarjeta);
            }
        } finally {
            listaPracticasTema.ResumeLayout(performLayout: true);
            desplazamientoPracticasTema.ActualizarContenido(volverAlInicio);
        }
    }

    private void ReconstruirDetallePractica(PracticaCurso practica, bool volverAlInicio = true) {
        contenidoDetallePractica.SuspendLayout();

        try {
            VaciarYDisponerControles(contenidoDetallePractica);
            int anchoContenido = Math.Max(
                320,
                contenidoDetallePractica.ClientSize.Width -
                contenidoDetallePractica.Padding.Horizontal -
                4);

        AgregarLabelFluido(
            contenidoDetallePractica,
            $"Práctica No. {practica.Numero}",
            anchoContenido,
            23,
            9F,
            FontStyle.Bold,
            ColorMoradoClaroCurso);
        AgregarLabelFluido(
            contenidoDetallePractica,
            practica.Nombre,
            anchoContenido,
            50,
            21F,
            FontStyle.Bold,
            Color.White);
        AgregarTextoDetalle(contenidoDetallePractica, practica.Descripcion, anchoContenido, 10F);

        AgregarSeccionDetalle(
            contenidoDetallePractica,
            "Objetivo",
            practica.Objetivo,
            anchoContenido);
        AgregarSeccionDetalle(
            contenidoDetallePractica,
            "Conceptos que practicarás",
            string.Join("   •   ", practica.Conceptos),
            anchoContenido);
        AgregarSeccionDetalle(
            contenidoDetallePractica,
            "Instrucciones generales",
            string.Join(Environment.NewLine, practica.Instrucciones.Select(
                (instruccion, indice) => $"{indice + 1}. {instruccion}")),
            anchoContenido);
        AgregarSeccionDetalle(
            contenidoDetallePractica,
            "Resultado esperado",
            practica.ResultadoEsperado,
            anchoContenido);
        AgregarSeccionDetalle(
            contenidoDetallePractica,
            "Dificultad",
            practica.Dificultad,
            anchoContenido);

        EstadoPracticaCurso estado = ObtenerEstadoPractica(practica.Id);
        AgregarSeccionDetalle(
            contenidoDetallePractica,
            "Estado actual",
            ObtenerTextoEstadoPractica(estado),
            anchoContenido,
            ObtenerColorEstado(estado));

        Button btnAccionPractica = CrearBotonCurso(
            ObtenerTextoAccionPractica(practica),
            Point.Empty,
            new Size(anchoContenido, 44),
            ColorMoradoCurso);
        btnAccionPractica.Margin = new Padding(0, 10, 0, 8);
        btnAccionPractica.Click += BtnAccionPracticaCurso_Click;
        contenidoDetallePractica.Controls.Add(btnAccionPractica);

        Panel panelAccionesEstado = new() {
            BackColor = Color.FromArgb(18, 14, 27),
            Margin = new Padding(0, 0, 0, 16),
            Size = new Size(anchoContenido, 42)
        };
        Button btnRealizada = CrearBotonSecundarioCurso(
            "Marcar como realizada",
            new Point(0, 0),
            new Size(260, 38));
        Button btnPendiente = CrearBotonSecundarioCurso(
            "Marcar como pendiente",
            new Point(274, 0),
            new Size(260, 38));
        btnRealizada.Click += (_, _) => CambiarEstadoPracticaCurso(EstadoPracticaCurso.Realizada);
        btnPendiente.Click += (_, _) => CambiarEstadoPracticaCurso(EstadoPracticaCurso.Pendiente);
        panelAccionesEstado.Controls.Add(btnRealizada);
        panelAccionesEstado.Controls.Add(btnPendiente);
        contenidoDetallePractica.Controls.Add(panelAccionesEstado);
        } finally {
            contenidoDetallePractica.ResumeLayout(performLayout: true);
            desplazamientoDetallePractica.ActualizarContenido(volverAlInicio);
        }
    }

    private void BtnAccionPracticaCurso_Click(object? sender, EventArgs e) {
        if (practicaCursoSeleccionada is null || temaCursoSeleccionado is null) {
            return;
        }

        ProgresoPractica? progreso = ObtenerProgresoPractica(practicaCursoSeleccionada.Id);
        string rutaProyecto = progreso?.RutaProyecto ?? string.Empty;
        bool proyectoExiste =
            !string.IsNullOrWhiteSpace(rutaProyecto) && Directory.Exists(rutaProyecto);

        if (proyectoExiste) {
            bool aperturaExitosa = IntentarAbrirPractica(rutaProyecto, promoverReciente: true);

            if (aperturaExitosa && progreso?.Estado != EstadoPracticaCurso.Realizada) {
                PersistirEstadoPractica(
                    practicaCursoSeleccionada.Id,
                    EstadoPracticaCurso.EnProgreso,
                    rutaProyecto);
            }

            if (aperturaExitosa) {
                ReconstruirDetallePractica(practicaCursoSeleccionada);
            }
            return;
        }

        ResultadoCreacionPractica? resultado = EjecutarCreacionPractica(
            temaCursoSeleccionado.NombreCarpeta,
            practicaCursoSeleccionada.NombreProyecto,
            practicaCursoSeleccionada.Objetivo,
            () => { },
            out rutaProyecto);

        if (resultado is null) {
            return;
        }

        if (resultado.Estado == EstadoCreacionPractica.ErrorApertura) {
            MostrarResultadoCreacionPractica(resultado, enfocarNombreProyecto: false);
            PersistirEstadoPractica(
                practicaCursoSeleccionada.Id,
                EstadoPracticaCurso.Pendiente,
                rutaProyecto);
            ReconstruirDetallePractica(practicaCursoSeleccionada);
            return;
        }

        if (!MostrarResultadoCreacionPractica(resultado, enfocarNombreProyecto: false)) {
            return;
        }

        PersistirEstadoPractica(
            practicaCursoSeleccionada.Id,
            EstadoPracticaCurso.EnProgreso,
            rutaProyecto);
        ReconstruirDetallePractica(practicaCursoSeleccionada);
    }

    private void CambiarEstadoPracticaCurso(EstadoPracticaCurso estado) {
        if (practicaCursoSeleccionada is null) {
            return;
        }

        if (!PersistirEstadoPractica(practicaCursoSeleccionada.Id, estado)) {
            return;
        }

        ReconstruirTarjetasTemas();

        if (temaCursoSeleccionado is not null) {
            ReconstruirTarjetasPracticas(temaCursoSeleccionado);
        }

        ReconstruirDetallePractica(practicaCursoSeleccionada);
        ActualizarResumenCurso();
    }

    private bool PersistirEstadoPractica(
        string practicaId,
        EstadoPracticaCurso estado,
        string? rutaProyecto = null) {
        ResultadoEscrituraProgreso resultado = progresoCursoService.ActualizarEstado(
            practicaId,
            estado,
            rutaProyecto);

        if (!resultado.EsExitosa) {
            if (!string.IsNullOrWhiteSpace(rutaProyecto)) {
                ActualizarProgresoEnMemoria(practicaId, estado, rutaProyecto);
            }

            string mensaje = resultado.Estado switch {
                EstadoEscrituraProgreso.PermisosInsuficientes =>
                    "No se pudo guardar el progreso porque no hay permisos para escribir progreso.json.",
                EstadoEscrituraProgreso.ContenidoInvalido =>
                    "No se pudo guardar el progreso porque progreso.json contiene información dañada que no puede recuperarse de forma segura.",
                _ =>
                    "No se pudo guardar el progreso. Verifica que progreso.json no esté bloqueado y que su carpeta permita crear y reemplazar archivos."
            };

            if (!string.IsNullOrWhiteSpace(rutaProyecto)) {
                mensaje += " La práctica seguirá disponible durante esta sesión.";
            }

            MessageBox.Show(mensaje, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        ActualizarProgresoEnMemoria(practicaId, estado, rutaProyecto);
        CargarProgresoCurso(mostrarAvisoInmediato: false);
        return true;
    }

    private void ActualizarProgresoEnMemoria(
        string practicaId,
        EstadoPracticaCurso estado,
        string? rutaProyecto) {
        ProgresoPractica? progreso = ObtenerProgresoPractica(practicaId);
        DateTimeOffset ahora = DateTimeOffset.Now;

        if (progreso is null) {
            progreso = new ProgresoPractica {
                PracticaId = practicaId,
                FechaCreacion = ahora
            };
            progresoCurso.Practicas.Add(progreso);
        }

        progreso.Estado = estado;

        if (!string.IsNullOrWhiteSpace(rutaProyecto)) {
            progreso.RutaProyecto = rutaProyecto;
        }

        progreso.FechaFinalizacion = estado == EstadoPracticaCurso.Realizada
            ? ahora
            : null;
    }

    private static string PrepararDescripcionTarjeta(string descripcion) {
        const int longitudPrimeraLinea = 38;

        if (descripcion.Length <= longitudPrimeraLinea) {
            return descripcion;
        }

        int separador = descripcion.LastIndexOf(' ', longitudPrimeraLinea);

        if (separador <= 0) {
            separador = descripcion.IndexOf(' ', longitudPrimeraLinea);
        }

        return separador > 0
            ? descripcion.Insert(separador, Environment.NewLine).Trim()
            : descripcion;
    }

    private string ObtenerTextoAccionPractica(PracticaCurso practica) {
        ProgresoPractica? progreso = ObtenerProgresoPractica(practica.Id);
        bool existe =
            !string.IsNullOrWhiteSpace(progreso?.RutaProyecto) &&
            Directory.Exists(progreso.RutaProyecto);

        if (!existe) {
            return "Crear práctica";
        }

        return progreso!.Estado == EstadoPracticaCurso.Realizada
            ? "Abrir práctica"
            : "Continuar práctica";
    }

    private ProgresoPractica? ObtenerProgresoPractica(string practicaId) {
        return progresoCurso.Practicas.FirstOrDefault(progreso =>
            progreso.PracticaId.Equals(practicaId, StringComparison.OrdinalIgnoreCase));
    }

    private EstadoPracticaCurso ObtenerEstadoPractica(string practicaId) {
        return ObtenerProgresoPractica(practicaId)?.Estado ?? EstadoPracticaCurso.Pendiente;
    }

    private int ContarPracticasRealizadas(TemaCurso tema) {
        HashSet<string> identificadores = tema.Practicas
            .Select(practica => practica.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return progresoCurso.Practicas.Count(progreso =>
            progreso.Estado == EstadoPracticaCurso.Realizada &&
            identificadores.Contains(progreso.PracticaId));
    }

    private EstadoPracticaCurso ObtenerEstadoTema(TemaCurso tema, int realizadas) {
        if (realizadas >= tema.TotalPracticasPlaneadas) {
            return EstadoPracticaCurso.Realizada;
        }

        bool enProgreso = tema.Practicas.Any(practica => {
            EstadoPracticaCurso estado = ObtenerEstadoPractica(practica.Id);
            return estado == EstadoPracticaCurso.EnProgreso ||
                estado == EstadoPracticaCurso.Realizada;
        });

        return enProgreso
            ? EstadoPracticaCurso.EnProgreso
            : EstadoPracticaCurso.Pendiente;
    }

    private static string ObtenerTextoEstadoTema(EstadoPracticaCurso estado) {
        return estado switch {
            EstadoPracticaCurso.Realizada => "✓ Completado",
            EstadoPracticaCurso.EnProgreso => "● En progreso",
            _ => "○ Pendiente"
        };
    }

    private static string ObtenerTextoEstadoPractica(EstadoPracticaCurso estado) {
        return estado switch {
            EstadoPracticaCurso.Realizada => "✓ Realizada",
            EstadoPracticaCurso.EnProgreso => "● En progreso",
            _ => "○ Pendiente"
        };
    }

    private static Color ObtenerColorEstado(EstadoPracticaCurso estado) {
        return estado switch {
            EstadoPracticaCurso.Realizada => Color.FromArgb(100, 214, 144),
            EstadoPracticaCurso.EnProgreso => Color.FromArgb(194, 128, 255),
            _ => Color.FromArgb(180, 174, 189)
        };
    }

    private static Panel CrearTarjetaCurso(Point ubicacion, Size tamano, int radio) {
        Panel tarjeta = new() {
            BackColor = ColorTarjetaCurso,
            Location = ubicacion,
            Size = tamano
        };

        AplicarRegionRedondeada(tarjeta, radio);
        tarjeta.SizeChanged += (_, _) => AplicarRegionRedondeada(tarjeta, radio);
        tarjeta.Paint += (_, e) => {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle limites = new(1, 1, tarjeta.Width - 3, tarjeta.Height - 3);

            using GraphicsPath contorno = CrearContornoRedondeado(limites, radio);
            using Pen borde = new(ColorBordeCurso, 1.2F);
            e.Graphics.DrawPath(borde, contorno);
        };

        return tarjeta;
    }

    private static void AplicarRegionRedondeada(Control control, int radio) {
        if (control.Width <= 0 || control.Height <= 0) {
            return;
        }

        using GraphicsPath contorno = CrearContornoRedondeado(
            new Rectangle(0, 0, control.Width, control.Height),
            radio);
        Region? regionAnterior = control.Region;
        control.Region = new Region(contorno);
        regionAnterior?.Dispose();
    }

    private static Label CrearLabelCurso(
        string texto,
        Point ubicacion,
        Size tamano,
        float tamanoFuente,
        FontStyle estilo,
        Color color,
        ContentAlignment alineacion = ContentAlignment.MiddleLeft) {
        Font fuente = new("Segoe UI", tamanoFuente, estilo);
        Label label = new() {
            AutoEllipsis = false,
            BackColor = Color.Transparent,
            Font = fuente,
            ForeColor = color,
            Location = ubicacion,
            Size = tamano,
            Text = texto,
            TextAlign = alineacion
        };
        label.Disposed += (_, _) => fuente.Dispose();
        return label;
    }

    private static Button CrearBotonCurso(
        string texto,
        Point ubicacion,
        Size tamano,
        Color colorFondo) {
        Font fuente = new("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        Button boton = new() {
            BackColor = colorFondo,
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat,
            Font = fuente,
            ForeColor = Color.White,
            Location = ubicacion,
            Size = tamano,
            Text = texto,
            UseVisualStyleBackColor = false
        };
        boton.FlatAppearance.BorderSize = 0;
        boton.FlatAppearance.MouseDownBackColor = Color.FromArgb(116, 55, 178);
        boton.FlatAppearance.MouseOverBackColor = Color.FromArgb(174, 108, 232);
        boton.Disposed += (_, _) => fuente.Dispose();
        return boton;
    }

    private static Button CrearBotonSecundarioCurso(string texto, Point ubicacion, Size tamano) {
        Font fuente = new("Segoe UI Semibold", 9F, FontStyle.Bold);
        Button boton = new() {
            BackColor = Color.FromArgb(35, 28, 48),
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat,
            Font = fuente,
            ForeColor = Color.FromArgb(220, 210, 232),
            Location = ubicacion,
            Size = tamano,
            Text = texto,
            UseVisualStyleBackColor = false
        };
        boton.FlatAppearance.BorderColor = ColorBordeCurso;
        boton.FlatAppearance.BorderSize = 1;
        boton.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 33, 62);
        boton.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 41, 74);
        boton.Disposed += (_, _) => fuente.Dispose();
        return boton;
    }

    private static void ConfigurarInteraccionTarjeta(Panel tarjeta, Action alHacerClick) {
        foreach (Control control in ObtenerArbolControles(tarjeta)) {
            control.Cursor = Cursors.Hand;
            control.Click += (_, _) => alHacerClick();
            control.MouseEnter += (_, _) => {
                tarjeta.BackColor = ColorTarjetaCursoHover;
                tarjeta.Invalidate();
            };
            control.MouseLeave += (_, _) => {
                if (!tarjeta.ClientRectangle.Contains(tarjeta.PointToClient(Cursor.Position))) {
                    tarjeta.BackColor = ColorTarjetaCurso;
                    tarjeta.Invalidate();
                }
            };
        }
    }

    private static IEnumerable<Control> ObtenerArbolControles(Control raiz) {
        yield return raiz;

        foreach (Control hijo in raiz.Controls) {
            foreach (Control descendiente in ObtenerArbolControles(hijo)) {
                yield return descendiente;
            }
        }
    }

    private static void VaciarYDisponerControles(Control contenedor) {
        Control[] controles = contenedor.Controls.Cast<Control>().ToArray();
        contenedor.Controls.Clear();

        foreach (Control control in controles) {
            control.Dispose();
        }
    }

    private static void AgregarLabelFluido(
        FlowLayoutPanel contenedor,
        string texto,
        int ancho,
        int alto,
        float tamanoFuente,
        FontStyle estilo,
        Color color) {
        Label label = CrearLabelCurso(
            texto,
            Point.Empty,
            new Size(ancho, alto),
            tamanoFuente,
            estilo,
            color);
        label.BackColor = contenedor.BackColor;
        label.Margin = new Padding(0, 0, 0, 2);
        contenedor.Controls.Add(label);
    }

    private static void AgregarTextoDetalle(
        FlowLayoutPanel contenedor,
        string texto,
        int ancho,
        float tamanoFuente,
        Color? color = null) {
        using Font fuenteMedicion = new("Segoe UI", tamanoFuente, FontStyle.Regular);
        int alto = Math.Max(
            30,
            TextRenderer.MeasureText(
                texto,
                fuenteMedicion,
                new Size(ancho - 8, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height + 10);

        Label label = CrearLabelCurso(
            texto,
            Point.Empty,
            new Size(ancho, alto),
            tamanoFuente,
            FontStyle.Regular,
            color ?? ColorTextoSecundarioCurso);
        label.BackColor = contenedor.BackColor;
        label.Margin = new Padding(0, 0, 0, 8);
        contenedor.Controls.Add(label);
    }

    private static void AgregarSeccionDetalle(
        FlowLayoutPanel contenedor,
        string titulo,
        string contenido,
        int ancho,
        Color? colorContenido = null) {
        AgregarLabelFluido(
            contenedor,
            titulo,
            ancho,
            25,
            10F,
            FontStyle.Bold,
            ColorMoradoClaroCurso);
        AgregarTextoDetalle(contenedor, contenido, ancho, 9.2F, colorContenido);
    }
}
