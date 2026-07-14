using EndForge.Models;

namespace EndForge;

public partial class frmPrincipal {
    private void GuardarProyectoReciente(string rutaProyecto) {
        recientesService.GuardarProyectoReciente(rutaProyecto);
    }

    private bool IntentarAbrirPractica(string rutaProyecto, bool promoverReciente = false) {
        ResultadoAperturaPractica resultado = aperturaPracticasService.AbrirPractica(rutaProyecto);

        if (resultado.Estado == EstadoAperturaPractica.CarpetaInexistente) {
            MessageBox.Show("La carpeta de esta práctica ya no existe.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (resultado.Estado != EstadoAperturaPractica.Exitosa) {
            MessageBox.Show("No se pudo abrir la práctica.\n\n" + resultado.Error!.Message, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (promoverReciente) {
            GuardarProyectoReciente(rutaProyecto);
            CargarRecientes();
        }

        return true;
    }

    private List<Label> ObtenerLabelsRecientes() {
        return new List<Label> {
            lblReciente1,
            lblReciente2,
            lblReciente3,
            lblReciente4,
            lblReciente5,
            lblReciente6,
            lblReciente7,
            lblReciente8,
            lblReciente9,
            lblReciente10
        };
    }

    private void LimpiarLabelsRecientes() {
        foreach (Label label in ObtenerLabelsRecientes()) {
            label.Text = "";
            label.Visible = false;
            label.Tag = null;
        }
    }

    private void LabelReciente_DoubleClick(object? sender, EventArgs e) {
        Label? label = sender as Label;

        if (label?.Tag is not ProyectoReciente proyecto)
            return;

        IntentarAbrirPractica(proyecto.Ruta, promoverReciente: true);
    }

    private void CargarRecientes() {
        listRecientes.Items.Clear();
        LimpiarLabelsRecientes();

        if (!recientesService.ExisteArchivoRecientes())
            return;

        string[] recientes = recientesService.LeerProyectosRecientes();

        if (recientes.Length > 0) {
            string[] datos = recientes[0].Split('|');

            if (datos.Length >= 2) {
                lblCardRecientesDesc.Text = datos[0];
                lblCardRecientesDesc.Tag = datos[1];

                lblCardContinuarDesc.Text = datos[0];
                lblCardContinuarDesc.Tag = datos[1];
            }
        } else {
            lblCardRecientesDesc.Text = "No hay proyectos recientes.";
            lblCardContinuarDesc.Text = "No hay prácticas recientes.";
        }

        List<Label> labelsRecientes = ObtenerLabelsRecientes();
        int indice = 0;

        foreach (string reciente in recientes) {
            string[] datos = reciente.Split('|');

            if (datos.Length >= 2) {
                ProyectoReciente proyecto = new ProyectoReciente {
                    Nombre = datos[0],
                    Ruta = datos[1]
                };

                listRecientes.Items.Add(proyecto);

                if (indice < labelsRecientes.Count) {
                    labelsRecientes[indice].Text = proyecto.Nombre;
                    labelsRecientes[indice].Tag = proyecto;
                    labelsRecientes[indice].Visible = true;
                    indice++;
                }
            }
        }
    }

    private void ListRecientes_DoubleClick(object sender, EventArgs e) {
        if (listRecientes.SelectedItem == null)
            return;

        ProyectoReciente proyecto = (ProyectoReciente)listRecientes.SelectedItem;
        IntentarAbrirPractica(proyecto.Ruta, promoverReciente: true);
    }

    private void ListRecientes_SelectedIndexChanged(object sender, EventArgs e) {
    }

    private void LblAyudaRecientes_Click(object sender, EventArgs e) {
    }

    private void ListRecientes_SelectedIndexChanged_1(object sender, EventArgs e) {
    }

    private void ListRecientes_DrawItem(object sender, DrawItemEventArgs e) {
        if (e.Index < 0)
            return;

        bool seleccionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Color colorFondo = seleccionado ? Color.FromArgb(111, 45, 189) : Color.FromArgb(20, 16, 30);
        Color colorTexto = Color.White;
        Color colorLinea = Color.FromArgb(55, 45, 70);

        using (SolidBrush fondo = new SolidBrush(colorFondo)) {
            e.Graphics.FillRectangle(fondo, e.Bounds);
        }

        string texto = listRecientes.Items[e.Index].ToString() ?? "";
        Rectangle areaTexto = new Rectangle(e.Bounds.Left + 12, e.Bounds.Top, e.Bounds.Width - 24, e.Bounds.Height - 1);

        TextRenderer.DrawText(e.Graphics, texto, listRecientes.Font, areaTexto, colorTexto, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

        using (Pen linea = new Pen(colorLinea)) {
            e.Graphics.DrawLine(linea, e.Bounds.Left + 8, e.Bounds.Bottom - 1, e.Bounds.Right - 8, e.Bounds.Bottom - 1);
        }
    }

    private void LblCardRecientesDesc_Click(object sender, EventArgs e) {
        string? rutaProyecto = lblCardRecientesDesc.Tag as string;

        if (!string.IsNullOrWhiteSpace(rutaProyecto)) {
            IntentarAbrirPractica(rutaProyecto);
        }
    }

    private void PanelCardRecientes_Click(object sender, EventArgs e) {
        LblCardRecientesDesc_Click(sender, e);
    }

    private void LblCardRecientesTitulo_Click(object sender, EventArgs e) {
        LblCardRecientesDesc_Click(sender, e);
    }

    private void PanelCardContinuar_Click(object sender, EventArgs e) {
        string? rutaProyecto = lblCardContinuarDesc.Tag?.ToString();

        if (string.IsNullOrWhiteSpace(rutaProyecto))
            return;

        IntentarAbrirPractica(rutaProyecto);
    }

    private void panelCardContinuar_Click(object sender, EventArgs e) {
    }

    private void TxtBuscarReciente_TextChanged(object sender, EventArgs e) {
        string filtro = txtBuscarReciente.Text.Trim().ToLower();

        listRecientes.Items.Clear();
        LimpiarLabelsRecientes();

        if (!recientesService.ExisteArchivoRecientes())
            return;

        string[] recientes = recientesService.LeerProyectosRecientes();
        List<Label> labelsRecientes = ObtenerLabelsRecientes();
        int indice = 0;

        foreach (string reciente in recientes) {
            string[] datos = reciente.Split('|');

            if (datos.Length < 2)
                continue;

            if (!datos[0].ToLower().Contains(filtro))
                continue;

            ProyectoReciente proyecto = new ProyectoReciente {
                Nombre = datos[0],
                Ruta = datos[1]
            };

            listRecientes.Items.Add(proyecto);

            if (indice < labelsRecientes.Count) {
                labelsRecientes[indice].Text = proyecto.Nombre;
                labelsRecientes[indice].Tag = proyecto;
                labelsRecientes[indice].Visible = true;
                indice++;
            }
        }
    }

    private void TxtBuscarReciente_Enter(object sender, EventArgs e) {
        if (txtBuscarReciente.Text == "Buscar práctica...") {
            txtBuscarReciente.Text = "";
            txtBuscarReciente.ForeColor = Color.White;
        }
    }

    private void TxtBuscarReciente_Leave(object sender, EventArgs e) {
        if (string.IsNullOrWhiteSpace(txtBuscarReciente.Text)) {
            txtBuscarReciente.Text = "Buscar práctica...";
            txtBuscarReciente.ForeColor = Color.Gray;
        }
    }
}
