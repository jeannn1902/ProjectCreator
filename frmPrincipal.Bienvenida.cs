using EndForge.Models;

namespace EndForge;

public partial class frmPrincipal {
    private Panel panelPantallaBienvenida = null!;
    private Panel panelContenidoBienvenida = null!;
    private Label lblLogoBienvenida = null!;
    private Label lblSubtituloBienvenida = null!;
    private Label lblContinuarBienvenida = null!;
    private System.Windows.Forms.Timer? timerBienvenida;
    private bool entradaAplicacionRealizada;
    private int intensidadContinuar = 120;
    private int direccionAnimacionBienvenida = 1;

    private void InicializarBienvenida() {
        panelPantallaBienvenida = new Panel {
            Name = "panelPantallaBienvenida",
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        panelContenidoBienvenida = new Panel {
            Name = "panelContenidoBienvenida",
            BackColor = Color.Transparent,
            Size = new Size(850, 250),
            Cursor = Cursors.Hand
        };

        lblLogoBienvenida = new Label {
            Name = "lblLogoBienvenida",
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI Semibold", 38F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(0, 5),
            Size = new Size(850, 105),
            Text = "EndForge",
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };

        lblSubtituloBienvenida = new Label {
            Name = "lblSubtituloBienvenida",
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 14F),
            ForeColor = Color.FromArgb(214, 200, 232),
            Location = new Point(0, 110),
            Size = new Size(850, 42),
            Text = "Tu entorno para aprender y crear en C++",
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };

        lblContinuarBienvenida = new Label {
            Name = "lblContinuarBienvenida",
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI Semibold", 10.5F),
            ForeColor = Color.FromArgb(intensidadContinuar, intensidadContinuar, intensidadContinuar),
            Location = new Point(0, 185),
            Size = new Size(850, 38),
            Text = "Presiona Enter o haz clic para comenzar",
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };

        panelContenidoBienvenida.Controls.Add(lblLogoBienvenida);
        panelContenidoBienvenida.Controls.Add(lblSubtituloBienvenida);
        panelContenidoBienvenida.Controls.Add(lblContinuarBienvenida);
        panelPantallaBienvenida.Controls.Add(panelContenidoBienvenida);

        panelPantallaBienvenida.Click += PantallaBienvenida_Click;
        panelContenidoBienvenida.Click += PantallaBienvenida_Click;
        lblLogoBienvenida.Click += PantallaBienvenida_Click;
        lblSubtituloBienvenida.Click += PantallaBienvenida_Click;
        lblContinuarBienvenida.Click += PantallaBienvenida_Click;
        panelBarraTitulo.Click += PantallaBienvenida_Click;
        lblBarraTitulo.Click += PantallaBienvenida_Click;
        pictureBoxBarraIcono.Click += PantallaBienvenida_Click;

        KeyPreview = true;
        KeyDown += FrmPrincipal_BienvenidaKeyDown;
        FormClosed += FrmPrincipal_BienvenidaFormClosed;

        fondoEndForge.Controls.Add(panelPantallaBienvenida);
        panelPantallaBienvenida.BringToFront();
        panelBarraTitulo.BringToFront();

        timerBienvenida = new System.Windows.Forms.Timer {
            Interval = 45
        };
        timerBienvenida.Tick += TimerBienvenida_Tick;
        timerBienvenida.Start();

        CentrarContenidoBienvenida();
        MostrarPantallaBienvenida();
    }

    private void MostrarPantallaBienvenida() {
        panelMenu.Visible = false;
        panelPrincipal.Visible = false;
        panelPantallaBienvenida.Visible = true;
        panelPantallaBienvenida.BringToFront();
        panelBarraTitulo.BringToFront();
        CentrarContenidoBienvenida();
    }

    private void CentrarContenidoBienvenida() {
        if (panelPantallaBienvenida.ClientSize.Width <= 0 ||
            panelPantallaBienvenida.ClientSize.Height <= 0) {
            return;
        }

        AjustarDisenoBienvenida();

        int x = Math.Max(0, (panelPantallaBienvenida.ClientSize.Width - panelContenidoBienvenida.Width) / 2);
        int y = Math.Max(
            panelBarraTitulo.Height,
            (panelPantallaBienvenida.ClientSize.Height - panelContenidoBienvenida.Height) / 2
        );

        panelContenidoBienvenida.Location = new Point(x, y);
    }

    private void AjustarDisenoBienvenida() {
        const int anchoMaximo = 850;
        const int margenHorizontal = 40;
        int anchoDisponible = Math.Max(
            1,
            panelPantallaBienvenida.ClientSize.Width - margenHorizontal * 2);
        int anchoContenido = Math.Min(anchoMaximo, anchoDisponible);

        Size medidaLogo = TextRenderer.MeasureText(
            lblLogoBienvenida.Text,
            lblLogoBienvenida.Font,
            new Size(anchoContenido, int.MaxValue),
            TextFormatFlags.SingleLine);
        int altoLogo = Math.Max(96, medidaLogo.Height + 24);

        Size medidaSubtitulo = TextRenderer.MeasureText(
            lblSubtituloBienvenida.Text,
            lblSubtituloBienvenida.Font,
            new Size(anchoContenido, int.MaxValue),
            TextFormatFlags.SingleLine);
        int altoSubtitulo = Math.Max(42, medidaSubtitulo.Height + 10);

        lblLogoBienvenida.SetBounds(0, 0, anchoContenido, altoLogo);
        lblSubtituloBienvenida.SetBounds(
            0,
            lblLogoBienvenida.Bottom + 8,
            anchoContenido,
            altoSubtitulo);
        lblContinuarBienvenida.SetBounds(
            0,
            lblSubtituloBienvenida.Bottom + 34,
            anchoContenido,
            38);
        panelContenidoBienvenida.Size = new Size(
            anchoContenido,
            lblContinuarBienvenida.Bottom + 8);
    }

    private void TimerBienvenida_Tick(object? sender, EventArgs e) {
        intensidadContinuar += direccionAnimacionBienvenida * 5;

        if (intensidadContinuar >= 235) {
            intensidadContinuar = 235;
            direccionAnimacionBienvenida = -1;
        } else if (intensidadContinuar <= 120) {
            intensidadContinuar = 120;
            direccionAnimacionBienvenida = 1;
        }

        int rojo = intensidadContinuar;
        int verde = Math.Max(105, intensidadContinuar - 10);
        int azul = Math.Min(255, intensidadContinuar + 20);
        lblContinuarBienvenida.ForeColor = Color.FromArgb(rojo, verde, azul);
    }

    private void PantallaBienvenida_Click(object? sender, EventArgs e) {
        EntrarAAplicacion();
    }

    private void FrmPrincipal_BienvenidaKeyDown(object? sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Enter) {
            e.Handled = true;
            e.SuppressKeyPress = true;
            EntrarAAplicacion();
            return;
        }

        if (e.KeyCode == Keys.Escape) {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }

    private void EntrarAAplicacion() {
        if (entradaAplicacionRealizada) {
            return;
        }

        entradaAplicacionRealizada = true;
        DetenerTemporizadorBienvenida();
        KeyDown -= FrmPrincipal_BienvenidaKeyDown;
        panelBarraTitulo.Click -= PantallaBienvenida_Click;
        lblBarraTitulo.Click -= PantallaBienvenida_Click;
        pictureBoxBarraIcono.Click -= PantallaBienvenida_Click;

        panelPantallaBienvenida.Visible = false;
        panelPrincipal.Visible = true;
        fondoEndForge.SendToBack();
        MostrarNavegacionPrincipal();

        ActiveControl = null;

        CompletarInicioAplicacion();
    }

    private void CompletarInicioAplicacion() {
        ResultadoCargaConfiguracion cargaConfiguracion = CargarConfiguracion();
        CargarTemas();
        CargarRecientes();
        CargarProgresoCurso();

        txtBuscarReciente.Text = "Buscar práctica...";
        txtBuscarReciente.ForeColor = Color.Gray;

        if (string.IsNullOrWhiteSpace(rutaBase) || string.IsNullOrWhiteSpace(rutaPlantilla)) {
            PanelConfiguracion_Click(panelConfiguracion, EventArgs.Empty);

            if (cargaConfiguracion.Estado == EstadoCargaConfiguracion.NoDisponible) {
                MessageBox.Show(
                    "¡Bienvenido a EndForge!\n\n" +
                    "Antes de comenzar, configura la carpeta de tus prácticas y la plantilla oficial.",
                    "Configuración inicial",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            return;
        }

        PanelInicio_Click(panelInicio, EventArgs.Empty);
    }

    private void DetenerTemporizadorBienvenida() {
        if (timerBienvenida == null) {
            return;
        }

        timerBienvenida.Stop();
        timerBienvenida.Tick -= TimerBienvenida_Tick;
        timerBienvenida.Dispose();
        timerBienvenida = null;
    }

    private void FrmPrincipal_BienvenidaFormClosed(object? sender, FormClosedEventArgs e) {
        DetenerTemporizadorBienvenida();
    }
}
