using EndForge.Models;
using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal {
    private PreferenciasAplicacion preferenciasAplicacion = new();
    private CheckBox chkMostrarTiemposOrientativos = null!;
    private Label lblAprendizajeConfiguracion = null!;
    private bool actualizandoPreferenciasAprendizaje;
    private string mensajeCargaPreferencias = string.Empty;

    private bool MostrarTiemposOrientativos =>
        preferenciasAplicacion.MostrarTiemposOrientativos;

    private void InicializarPreferenciasAprendizaje() {
        int x = txtRutaPlantillaConfig.Left;
        int ancho = txtRutaPlantillaConfig.Width;
        int ySeccion = txtRutaPlantillaConfig.Bottom + EscalarDiseno(7);
        int altoSeccion = EscalarDiseno(18);
        int altoOpcion = Math.Max(
            EscalarDiseno(22),
            btnGuardarConfiguracion.Top - ySeccion - altoSeccion - EscalarDiseno(3));
        lblAprendizajeConfiguracion = new Label {
            Name = "lblAprendizajeConfiguracion",
            AutoSize = false,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI Semibold", 9F),
            ForeColor = Color.FromArgb(202, 151, 247),
            Location = new Point(x, ySeccion),
            Size = new Size(ancho, altoSeccion),
            TabIndex = 5,
            Text = "APRENDIZAJE",
            TextAlign = ContentAlignment.MiddleLeft
        };

        chkMostrarTiemposOrientativos = new CheckBox {
            Name = "chkMostrarTiemposOrientativos",
            AutoSize = false,
            BackColor = Color.Transparent,
            Checked = true,
            CheckState = CheckState.Checked,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.White,
            Location = new Point(x, ySeccion + altoSeccion),
            Size = new Size(ancho, altoOpcion),
            TabIndex = 6,
            Text = "Mostrar tiempos orientativos",
            UseVisualStyleBackColor = false,
            AccessibleDescription =
                "Muestra u oculta los tiempos orientativos del curso. No limita el tiempo para resolver."
        };
        chkMostrarTiemposOrientativos.CheckedChanged +=
            ChkMostrarTiemposOrientativos_CheckedChanged;

        panelConfiguracionVista.Controls.Add(lblAprendizajeConfiguracion);
        panelConfiguracionVista.Controls.Add(chkMostrarTiemposOrientativos);
        txtRutaBaseConfig.TabIndex = 0;
        btnCambiarRutaBase.TabIndex = 1;
        txtRutaPlantillaConfig.TabIndex = 2;
        btnCambiarRutaPlantilla.TabIndex = 3;
        chkMostrarTiemposOrientativos.TabIndex = 4;
        btnGuardarConfiguracion.TabIndex = 5;
        btnRestaurarConfiguracion.TabIndex = 6;
        lblAprendizajeConfiguracion.BringToFront();
        chkMostrarTiemposOrientativos.BringToFront();
    }

    private void AplicarPreferenciasAprendizaje(
        ResultadoCargaPreferencias resultado,
        bool mostrarAviso) {
        preferenciasAplicacion = resultado.Preferencias;
        mensajeCargaPreferencias = resultado.Estado switch {
            EstadoCargaPreferencias.ContenidoInvalido =>
                "No se pudo cargar preferencias.json porque su contenido no es válido. El archivo no se modificará.",
            EstadoCargaPreferencias.VersionNoCompatible =>
                "preferencias.json pertenece a una versión más reciente de EndForge. El archivo no se modificará.",
            EstadoCargaPreferencias.PermisosInsuficientes =>
                "No hay permisos para leer las preferencias de aprendizaje. Se usarán los valores predeterminados.",
            EstadoCargaPreferencias.ErrorIo =>
                "No se pudieron leer las preferencias de aprendizaje porque el archivo está bloqueado o no está disponible.",
            _ => string.Empty
        };

        actualizandoPreferenciasAprendizaje = true;

        try {
            chkMostrarTiemposOrientativos.Checked =
                preferenciasAplicacion.MostrarTiemposOrientativos;
        } finally {
            actualizandoPreferenciasAprendizaje = false;
        }

        ActualizarEstadoCambiosConfiguracion();

        if (mostrarAviso) {
            MostrarAvisoPreferenciasSiCorresponde();
        }
    }

    private void CargarPreferenciasAprendizaje(bool mostrarAviso = false) {
        AplicarPreferenciasAprendizaje(preferenciasService.Cargar(), mostrarAviso);
    }

    private void MostrarAvisoPreferenciasSiCorresponde() {
        if (string.IsNullOrWhiteSpace(mensajeCargaPreferencias)) {
            return;
        }

        lblEstadoConfiguracion.Text = mensajeCargaPreferencias;
        lblEstadoConfiguracion.ForeColor = Color.LightCoral;
        lblEstadoConfiguracion.Visible = true;
    }

    private void ChkMostrarTiemposOrientativos_CheckedChanged(
        object? sender,
        EventArgs e) {
        if (actualizandoPreferenciasAprendizaje) {
            return;
        }

        bool hayCambios = ActualizarEstadoCambiosConfiguracion();

        if (hayCambios) {
            lblEstadoConfiguracion.Visible = false;
        }
    }

    private bool HayCambiosPreferenciasAprendizaje() {
        return chkMostrarTiemposOrientativos.Checked !=
            preferenciasAplicacion.MostrarTiemposOrientativos;
    }

    private bool GuardarPreferenciasAprendizajePendientes() {
        if (!HayCambiosPreferenciasAprendizaje()) {
            return true;
        }

        bool mostrarTiemposOrientativos =
            chkMostrarTiemposOrientativos.Checked;
        PreferenciasAplicacion nuevasPreferencias = new() {
            Version = preferenciasAplicacion.Version,
            MostrarTiemposOrientativos = mostrarTiemposOrientativos
        };
        ResultadoEscrituraPreferencias resultado =
            preferenciasService.Guardar(nuevasPreferencias);

        if (!resultado.EsExitosa) {
            lblEstadoConfiguracion.Text = resultado.Estado switch {
                EstadoEscrituraPreferencias.ContenidoInvalido =>
                    "No se guardó la preferencia porque preferencias.json está dañado. El archivo anterior se conservó.",
                EstadoEscrituraPreferencias.VersionNoCompatible =>
                    "No se guardó la preferencia porque preferencias.json pertenece a una versión más reciente. El archivo anterior se conservó.",
                EstadoEscrituraPreferencias.PermisosInsuficientes =>
                    "No se guardó la preferencia porque no hay permisos. El valor anterior se conservó.",
                _ =>
                    "No se guardó la preferencia porque preferencias.json está bloqueado o no está disponible. El valor anterior se conservó."
            };
            lblEstadoConfiguracion.ForeColor = Color.LightCoral;
            lblEstadoConfiguracion.Visible = true;
            return false;
        }

        ResultadoCargaPreferencias preferenciasPersistidas =
            preferenciasService.Cargar();

        if (preferenciasPersistidas.Estado != EstadoCargaPreferencias.Exitosa ||
            preferenciasPersistidas.Preferencias.MostrarTiemposOrientativos !=
                mostrarTiemposOrientativos) {
            lblEstadoConfiguracion.Text =
                "No se pudo confirmar la preferencia guardada. Vuelve a intentarlo.";
            lblEstadoConfiguracion.ForeColor = Color.LightCoral;
            lblEstadoConfiguracion.Visible = true;
            return false;
        }

        preferenciasAplicacion = preferenciasPersistidas.Preferencias;
        mensajeCargaPreferencias = string.Empty;
        ActualizarPresentacionTiemposOrientativos();
        return true;
    }

    private void RestaurarPreferenciasAprendizajeEnVista() {
        actualizandoPreferenciasAprendizaje = true;

        try {
            chkMostrarTiemposOrientativos.Checked =
                preferenciasAplicacion.MostrarTiemposOrientativos;
        } finally {
            actualizandoPreferenciasAprendizaje = false;
        }
    }

    private void ActualizarPresentacionTiemposOrientativos() {
        if (!cursoPreparado) {
            return;
        }

        ActualizarPracticaRecomendada();

        if (temaCursoSeleccionado is not null && panelPracticasTemaVista.Visible) {
            ReconstruirTarjetasPracticas(temaCursoSeleccionado, volverAlInicio: false);
        }

        if (practicaCursoSeleccionada is not null && panelDetallePracticaVista.Visible) {
            ReconstruirDetallePractica(practicaCursoSeleccionada, volverAlInicio: false);
        }
    }
}
