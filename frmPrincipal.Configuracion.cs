using EndForge.Models;

namespace EndForge;

public partial class frmPrincipal {
    private void CargarConfiguracion() {
        ResultadoCargaConfiguracion resultado = configuracionService.CargarConfiguracion();
        rutaBase = resultado.RutaBase;
        rutaPlantilla = resultado.RutaPlantilla;

        if (resultado.Estado == EstadoCargaConfiguracion.ErrorPermisosConfiguracion) {
            MessageBox.Show(
                "No se pudo leer la configuración de EndForge porque no hay permisos para acceder a config.txt.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (resultado.Estado == EstadoCargaConfiguracion.ErrorLecturaConfiguracion) {
            MessageBox.Show("No se pudo leer la configuración. Verifica que config.txt no esté bloqueado o en uso por otra aplicación.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!resultado.ConfiguracionDisponible) {
            return;
        }

        if (resultado.Estado == EstadoCargaConfiguracion.ErrorPermisosRecientes) {
            MessageBox.Show("No se pudo crear recientes.txt porque no hay permisos de acceso.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        } else if (resultado.Estado == EstadoCargaConfiguracion.ErrorCreacionRecientes) {
            MessageBox.Show("No se pudo crear recientes.txt porque el archivo está bloqueado o en uso.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        ValidarFormulario();

        if (!string.IsNullOrWhiteSpace(rutaBase) && !string.IsNullOrWhiteSpace(rutaPlantilla)) {
            lblEstadoConfiguracion.Text = "✅ Configuración lista.";
            lblEstadoConfiguracion.ForeColor = Color.LightGreen;
            lblEstadoConfiguracion.Visible = true;
        } else {
            lblEstadoConfiguracion.Visible = false;
        }
    }

    private void BtnCambiarRutaPlantilla_Click(object sender, EventArgs e) {
        using (FolderBrowserDialog carpeta = new FolderBrowserDialog()) {
            carpeta.Description = "Selecciona la carpeta de la plantilla";

            if (carpeta.ShowDialog() == DialogResult.OK) {
                txtRutaPlantillaConfig.Text = carpeta.SelectedPath;

                btnGuardarConfiguracion.Enabled = true;
                lblEstadoConfiguracion.Visible = false;
            }
        }
    }

    private void BtnCambiarRutaBase_Click(object sender, EventArgs e) {
        using (FolderBrowserDialog carpeta = new FolderBrowserDialog()) {
            carpeta.Description = "Selecciona la carpeta base de tus proyectos";

            if (carpeta.ShowDialog() == DialogResult.OK) {
                txtRutaBaseConfig.Text = carpeta.SelectedPath;

                btnGuardarConfiguracion.Enabled = true;
                lblEstadoConfiguracion.Visible = false;
            }
        }
    }

    private void BtnGuardarConfiguracion_Click(object sender, EventArgs e) {
        lblEstadoConfiguracion.Visible = false;
        EstadoValidacionConfiguracion validacion = configuracionService.ValidarConfiguracion(
            txtRutaBaseConfig.Text,
            txtRutaPlantillaConfig.Text
        );

        if (validacion != EstadoValidacionConfiguracion.Valida) {
            lblEstadoConfiguracion.Text = validacion switch {
                EstadoValidacionConfiguracion.RutasNoExistentes => "❌ Una de las rutas seleccionadas no existe.",
                EstadoValidacionConfiguracion.PlantillaSinSolucion => "❌ Plantilla de EndForge no válida.",
                EstadoValidacionConfiguracion.PlantillaSinProyectoCpp => "❌ No se encontró un proyecto C++.",
                _ => "❌ No se encontraron archivos C++."
            };
            lblEstadoConfiguracion.ForeColor = validacion == EstadoValidacionConfiguracion.RutasNoExistentes
                ? Color.IndianRed
                : Color.LightCoral;
            lblEstadoConfiguracion.Visible = true;
            return;
        }

        try {
            configuracionService.GuardarConfiguracion(
                txtRutaBaseConfig.Text,
                txtRutaPlantillaConfig.Text
            );
        } catch (Exception ex) {
            lblEstadoConfiguracion.Text = "❌ No se pudieron guardar los cambios.\n" + ex.Message;
            lblEstadoConfiguracion.ForeColor = Color.LightCoral;
            lblEstadoConfiguracion.Visible = true;
            return;
        }

        CargarConfiguracion();

        lblEstadoConfiguracion.Text = "✅ Cambios guardados.";
        lblEstadoConfiguracion.ForeColor = Color.LightGreen;
        lblEstadoConfiguracion.Visible = true;

        btnGuardarConfiguracion.Enabled = false;

        // PanelInicio_Click(panelInicio, EventArgs.Empty);
        CargarRecientes();
    }
}
