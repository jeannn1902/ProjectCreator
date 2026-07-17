using System.Drawing.Drawing2D;

namespace EndForge.Controls;

internal sealed class PanelDesplazableSinBarras : Panel {
    private const int DeltaRueda = 120;
    private const int PasoLinea = 40;
    private const int AnchoReservaBarra = 16;
    private const int AnchoPistaBarra = 8;
    private const int MargenVerticalBarra = 4;
    private const int AltoMinimoIndicador = 28;

    private static readonly Color ColorPistaBarra = Color.FromArgb(31, 25, 45);
    private static readonly Color ColorIndicadorBarra = Color.FromArgb(145, 82, 214);
    private static readonly Color ColorIndicadorBarraHover = Color.FromArgb(174, 108, 232);

    private int desplazamientoVertical;
    private int acumuladoRueda;
    private bool actualizandoContenido;
    private bool barraVisible;
    private bool indicadorHover;
    private bool arrastrandoIndicador;
    private int desfaseArrastreIndicador;

    public FlowLayoutPanel Contenido { get; }

    public Color ColorFondoContenido {
        get => Contenido.BackColor;
        set {
            Contenido.BackColor = value;
            Contenido.Invalidate(true);
            InvalidarBarra();
        }
    }

    public PanelDesplazableSinBarras() {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable,
            true);

        DoubleBuffered = true;
        TabStop = true;
        BackColor = Color.FromArgb(116, 67, 163);
        Padding = new Padding(1);

        Contenido = new FlowLayoutPanel {
            AutoScroll = false,
            BackColor = Color.FromArgb(18, 14, 27),
            FlowDirection = FlowDirection.TopDown,
            Location = new Point(Padding.Left, Padding.Top),
            Margin = Padding.Empty,
            TabStop = false,
            WrapContents = false
        };
        Contenido.SetStyleDobleBuffer();

        Controls.Add(Contenido);
        RegistrarArbol(Contenido);
        Contenido.Layout += Contenido_Layout;
    }

    public void ActualizarContenido(bool volverAlInicio) {
        if (actualizandoContenido || IsDisposed) {
            return;
        }

        actualizandoContenido = true;

        try {
            if (volverAlInicio) {
                desplazamientoVertical = 0;
                acumuladoRueda = 0;
            }

            int anchoDisponible = Math.Max(
                1,
                ClientSize.Width - Padding.Horizontal - AnchoReservaBarra);
            int altoDisponible = Math.Max(1, ClientSize.Height - Padding.Vertical);
            int altoContenido = Contenido.Padding.Vertical;

            foreach (Control control in Contenido.Controls) {
                if (control.Visible) {
                    altoContenido += control.Height + control.Margin.Vertical;
                }
            }

            barraVisible = altoContenido > altoDisponible;
            altoContenido = Math.Max(altoDisponible, altoContenido);
            int maximo = Math.Max(0, altoContenido - altoDisponible);
            desplazamientoVertical = Math.Clamp(desplazamientoVertical, 0, maximo);

            if (!barraVisible) {
                indicadorHover = false;
                arrastrandoIndicador = false;
                desfaseArrastreIndicador = 0;
                Cursor = Cursors.Default;

                if (Capture) {
                    Capture = false;
                }
            }

            Contenido.SetBounds(
                Padding.Left,
                Padding.Top - desplazamientoVertical,
                anchoDisponible,
                altoContenido);
            Contenido.PerformLayout();
            Invalidate();
        } finally {
            actualizandoContenido = false;
        }
    }

    public void IrAlInicio() {
        desplazamientoVertical = 0;
        acumuladoRueda = 0;
        AplicarDesplazamiento();
    }

    public void IrAlFinal() {
        MoverA(int.MaxValue);
    }

    protected override void OnResize(EventArgs e) {
        base.OnResize(e);
        ActualizarContenido(volverAlInicio: false);
    }

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);

        Rectangle reservaBarra = ObtenerRectanguloReservaBarra();

        if (reservaBarra.IsEmpty) {
            return;
        }

        using (SolidBrush fondo = new(ColorFondoContenido)) {
            e.Graphics.FillRectangle(fondo, reservaBarra);
        }

        if (!barraVisible) {
            return;
        }

        Rectangle pista = ObtenerRectanguloPista();
        Rectangle indicador = ObtenerRectanguloIndicador();

        if (pista.IsEmpty || indicador.IsEmpty) {
            return;
        }

        SmoothingMode suavizadoAnterior = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using (SolidBrush fondoPista = new(ColorPistaBarra)) {
            RellenarRectanguloRedondeado(e.Graphics, fondoPista, pista);
        }

        Color colorIndicador = indicadorHover || arrastrandoIndicador
            ? ColorIndicadorBarraHover
            : ColorIndicadorBarra;

        using (SolidBrush fondoIndicador = new(colorIndicador)) {
            RellenarRectanguloRedondeado(e.Graphics, fondoIndicador, indicador);
        }

        e.Graphics.SmoothingMode = suavizadoAnterior;
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left || !barraVisible) {
            return;
        }

        Rectangle indicador = ObtenerRectanguloIndicador();
        Rectangle pista = ObtenerRectanguloPista();

        if (indicador.Contains(e.Location)) {
            IniciarArrastreIndicador(e.Y - indicador.Top);
            return;
        }

        if (pista.Contains(e.Location)) {
            IniciarArrastreIndicador(indicador.Height / 2);
            MoverIndicadorDesdeMouse(e.Y);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);

        if (arrastrandoIndicador) {
            MoverIndicadorDesdeMouse(e.Y);
            return;
        }

        bool nuevoHover = barraVisible && ObtenerRectanguloIndicador().Contains(e.Location);

        if (nuevoHover == indicadorHover) {
            return;
        }

        indicadorHover = nuevoHover;
        Cursor = indicadorHover ? Cursors.Hand : Cursors.Default;
        InvalidarBarra();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);

        if (e.Button == MouseButtons.Left) {
            FinalizarArrastreIndicador(e.Location);
        }
    }

    protected override void OnMouseLeave(EventArgs e) {
        base.OnMouseLeave(e);

        if (!arrastrandoIndicador && indicadorHover) {
            indicadorHover = false;
            Cursor = Cursors.Default;
            InvalidarBarra();
        }
    }

    protected override void OnMouseCaptureChanged(EventArgs e) {
        base.OnMouseCaptureChanged(e);

        if (arrastrandoIndicador && !Capture) {
            arrastrandoIndicador = false;
            desfaseArrastreIndicador = 0;
            indicadorHover = false;
            Cursor = Cursors.Default;
            InvalidarBarra();
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        ProcesarRueda(e);

        if (e is HandledMouseEventArgs handled) {
            handled.Handled = true;
        }

        base.OnMouseWheel(e);
    }

    protected override bool IsInputKey(Keys keyData) {
        return (keyData & Keys.KeyCode) switch {
            Keys.Up or Keys.Down or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End => true,
            _ => base.IsInputKey(keyData)
        };
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        int pagina = Math.Max(PasoLinea, ClientSize.Height - PasoLinea);
        int? destino = e.KeyCode switch {
            Keys.Up => desplazamientoVertical - PasoLinea,
            Keys.Down => desplazamientoVertical + PasoLinea,
            Keys.PageUp => desplazamientoVertical - pagina,
            Keys.PageDown => desplazamientoVertical + pagina,
            Keys.Home => 0,
            Keys.End => int.MaxValue,
            _ => null
        };

        if (destino.HasValue) {
            MoverA(destino.Value);
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void ProcesarRueda(MouseEventArgs e) {
        int lineas = SystemInformation.MouseWheelScrollLines;

        if (lineas == 0) {
            return;
        }

        acumuladoRueda += e.Delta;
        int pasos = acumuladoRueda / DeltaRueda;
        acumuladoRueda %= DeltaRueda;

        if (pasos == 0) {
            return;
        }

        int distancia = lineas < 0
            ? Math.Max(PasoLinea, ClientSize.Height - PasoLinea)
            : lineas * PasoLinea;

        MoverA(desplazamientoVertical - pasos * distancia);
    }

    private void MoverA(int destino) {
        int altoDisponible = Math.Max(1, ClientSize.Height - Padding.Vertical);
        int maximo = Math.Max(0, Contenido.Height - altoDisponible);
        int nuevoValor = Math.Clamp(destino, 0, maximo);

        if (nuevoValor == desplazamientoVertical) {
            return;
        }

        desplazamientoVertical = nuevoValor;
        AplicarDesplazamiento();
    }

    private void AplicarDesplazamiento() {
        Contenido.Top = Padding.Top - desplazamientoVertical;
        Contenido.Invalidate(true);
        InvalidarBarra();
    }

    private void IniciarArrastreIndicador(int desfase) {
        Rectangle indicador = ObtenerRectanguloIndicador();

        if (indicador.IsEmpty) {
            return;
        }

        if (CanFocus) {
            Focus();
        }

        arrastrandoIndicador = true;
        indicadorHover = true;
        desfaseArrastreIndicador = Math.Clamp(desfase, 0, indicador.Height);
        Cursor = Cursors.Hand;
        Capture = true;
        InvalidarBarra();
    }

    private void MoverIndicadorDesdeMouse(int posicionY) {
        Rectangle pista = ObtenerRectanguloPista();
        Rectangle indicador = ObtenerRectanguloIndicador();
        int recorrido = pista.Height - indicador.Height;
        int maximo = ObtenerMaximoDesplazamiento();

        if (recorrido <= 0 || maximo <= 0) {
            MoverA(0);
            return;
        }

        int posicionIndicador = Math.Clamp(
            posicionY - desfaseArrastreIndicador - pista.Top,
            0,
            recorrido);
        int destino = (int)Math.Round(posicionIndicador * maximo / (double)recorrido);
        MoverA(destino);
    }

    private void FinalizarArrastreIndicador(Point ubicacionMouse) {
        if (!arrastrandoIndicador) {
            return;
        }

        arrastrandoIndicador = false;
        desfaseArrastreIndicador = 0;
        indicadorHover = barraVisible && ObtenerRectanguloIndicador().Contains(ubicacionMouse);
        Cursor = indicadorHover ? Cursors.Hand : Cursors.Default;

        if (Capture) {
            Capture = false;
        }

        InvalidarBarra();
    }

    private int ObtenerMaximoDesplazamiento() {
        int altoDisponible = Math.Max(1, ClientSize.Height - Padding.Vertical);
        return Math.Max(0, Contenido.Height - altoDisponible);
    }

    private Rectangle ObtenerRectanguloReservaBarra() {
        int limiteDerecho = Math.Max(Padding.Left, ClientSize.Width - Padding.Right);
        int x = Math.Max(Padding.Left, limiteDerecho - AnchoReservaBarra);
        int alto = Math.Max(0, ClientSize.Height - Padding.Vertical);

        return alto > 0 && limiteDerecho > x
            ? new Rectangle(x, Padding.Top, limiteDerecho - x, alto)
            : Rectangle.Empty;
    }

    private Rectangle ObtenerRectanguloPista() {
        Rectangle reserva = ObtenerRectanguloReservaBarra();
        int alto = reserva.Height - MargenVerticalBarra * 2;

        if (reserva.IsEmpty || alto <= 0) {
            return Rectangle.Empty;
        }

        int ancho = Math.Min(AnchoPistaBarra, reserva.Width);
        int x = reserva.Left + (reserva.Width - ancho) / 2;
        return new Rectangle(x, reserva.Top + MargenVerticalBarra, ancho, alto);
    }

    private Rectangle ObtenerRectanguloIndicador() {
        if (!barraVisible) {
            return Rectangle.Empty;
        }

        Rectangle pista = ObtenerRectanguloPista();
        int altoDisponible = Math.Max(1, ClientSize.Height - Padding.Vertical);
        int altoContenido = Math.Max(altoDisponible, Contenido.Height);

        if (pista.IsEmpty || altoContenido <= altoDisponible) {
            return Rectangle.Empty;
        }

        int altoMinimo = Math.Min(AltoMinimoIndicador, pista.Height);
        int altoIndicador = Math.Clamp(
            (int)Math.Round(pista.Height * altoDisponible / (double)altoContenido),
            altoMinimo,
            pista.Height);
        int recorrido = pista.Height - altoIndicador;
        int maximo = ObtenerMaximoDesplazamiento();
        int y = pista.Top;

        if (recorrido > 0 && maximo > 0) {
            y += (int)Math.Round(recorrido * desplazamientoVertical / (double)maximo);
        }

        return new Rectangle(pista.Left, y, pista.Width, altoIndicador);
    }

    private void InvalidarBarra() {
        Rectangle reserva = ObtenerRectanguloReservaBarra();

        if (!reserva.IsEmpty) {
            Invalidate(reserva);
        }
    }

    private static void RellenarRectanguloRedondeado(
        Graphics graphics,
        Brush brush,
        Rectangle rectangulo) {
        int diametro = Math.Min(rectangulo.Width, rectangulo.Height);

        if (diametro <= 1) {
            graphics.FillRectangle(brush, rectangulo);
            return;
        }

        using GraphicsPath trazado = new();
        trazado.AddArc(rectangulo.Left, rectangulo.Top, diametro, diametro, 180, 90);
        trazado.AddArc(rectangulo.Right - diametro, rectangulo.Top, diametro, diametro, 270, 90);
        trazado.AddArc(
            rectangulo.Right - diametro,
            rectangulo.Bottom - diametro,
            diametro,
            diametro,
            0,
            90);
        trazado.AddArc(rectangulo.Left, rectangulo.Bottom - diametro, diametro, diametro, 90, 90);
        trazado.CloseFigure();
        graphics.FillPath(brush, trazado);
    }

    private void Contenido_Layout(object? sender, LayoutEventArgs e) {
        if (!actualizandoContenido) {
            ActualizarContenido(volverAlInicio: false);
        }
    }

    private void RegistrarArbol(Control control) {
        control.MouseEnter += Control_MouseEnter;
        control.MouseWheel += Control_MouseWheel;
        control.ControlAdded += Control_ControlAdded;
        control.ControlRemoved += Control_ControlRemoved;

        foreach (Control hijo in control.Controls) {
            RegistrarArbol(hijo);
        }
    }

    private void DesregistrarArbol(Control control) {
        foreach (Control hijo in control.Controls) {
            DesregistrarArbol(hijo);
        }

        control.MouseEnter -= Control_MouseEnter;
        control.MouseWheel -= Control_MouseWheel;
        control.ControlAdded -= Control_ControlAdded;
        control.ControlRemoved -= Control_ControlRemoved;
    }

    private void Control_MouseEnter(object? sender, EventArgs e) {
        if (CanFocus && !ContainsFocus) {
            Focus();
        }
    }

    private void Control_MouseWheel(object? sender, MouseEventArgs e) {
        ProcesarRueda(e);

        if (e is HandledMouseEventArgs handled) {
            handled.Handled = true;
        }
    }

    private void Control_ControlAdded(object? sender, ControlEventArgs e) {
        if (e.Control is not null) {
            RegistrarArbol(e.Control);
        }
    }

    private void Control_ControlRemoved(object? sender, ControlEventArgs e) {
        if (e.Control is not null) {
            DesregistrarArbol(e.Control);
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            Contenido.Layout -= Contenido_Layout;
            DesregistrarArbol(Contenido);
        }

        base.Dispose(disposing);
    }
}

internal static class ControlDobleBufferExtensions {
    public static void SetStyleDobleBuffer(this Control control) {
        typeof(Control)
            .GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            ?.SetValue(control, true, null);
    }
}
