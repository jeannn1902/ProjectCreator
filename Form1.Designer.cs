namespace ProjectCreator
{
    partial class frmPrincipal
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPrincipal));
            lblTema = new Label();
            cmbTemas = new ComboBox();
            lblNombre = new Label();
            txtNombreProyecto = new TextBox();
            lblVistaPrevia = new Label();
            lblNombreFinal = new Label();
            btnCrearProyecto = new Button();
            lblTitulo = new Label();
            lblObjetivo = new Label();
            txtObjetivo = new TextBox();
            pictureBoxfondo = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBoxfondo).BeginInit();
            SuspendLayout();
            // 
            // lblTema
            // 
            lblTema.AutoSize = true;
            lblTema.BackColor = SystemColors.Control;
            lblTema.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTema.ForeColor = Color.DarkSlateGray;
            lblTema.Location = new Point(208, 85);
            lblTema.Name = "lblTema";
            lblTema.Size = new Size(62, 25);
            lblTema.TabIndex = 0;
            lblTema.Text = "Tema:";
            lblTema.Click += Label1_Click_1;
            // 
            // cmbTemas
            // 
            cmbTemas.Font = new Font("Segoe UI Light", 9F);
            cmbTemas.FormattingEnabled = true;
            cmbTemas.Location = new Point(114, 113);
            cmbTemas.Name = "cmbTemas";
            cmbTemas.Size = new Size(250, 33);
            cmbTemas.TabIndex = 1;
            cmbTemas.SelectedIndexChanged += CmbTemas_SelectedIndexChanged;
            // 
            // lblNombre
            // 
            lblNombre.AutoSize = true;
            lblNombre.BackColor = SystemColors.Control;
            lblNombre.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNombre.ForeColor = Color.DarkSlateGray;
            lblNombre.Location = new Point(143, 158);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(193, 25);
            lblNombre.TabIndex = 2;
            lblNombre.Text = "Nombre del ejercicio:";
            // 
            // txtNombreProyecto
            // 
            txtNombreProyecto.Font = new Font("Segoe UI Light", 9F);
            txtNombreProyecto.Location = new Point(114, 185);
            txtNombreProyecto.Name = "txtNombreProyecto";
            txtNombreProyecto.Size = new Size(250, 31);
            txtNombreProyecto.TabIndex = 3;
            txtNombreProyecto.TextChanged += TxtNombreProyecto_TextChanged;
            // 
            // lblVistaPrevia
            // 
            lblVistaPrevia.AutoSize = true;
            lblVistaPrevia.BackColor = SystemColors.Control;
            lblVistaPrevia.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblVistaPrevia.ForeColor = Color.DarkSlateGray;
            lblVistaPrevia.Location = new Point(181, 319);
            lblVistaPrevia.Name = "lblVistaPrevia";
            lblVistaPrevia.Size = new Size(117, 25);
            lblVistaPrevia.TabIndex = 4;
            lblVistaPrevia.Text = "Vista previa:";
            lblVistaPrevia.Click += LblVistaPrevia_Click;
            // 
            // lblNombreFinal
            // 
            lblNombreFinal.AutoSize = true;
            lblNombreFinal.BackColor = SystemColors.Control;
            lblNombreFinal.Font = new Font("Consolas", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblNombreFinal.ForeColor = Color.SteelBlue;
            lblNombreFinal.Location = new Point(139, 345);
            lblNombreFinal.Name = "lblNombreFinal";
            lblNombreFinal.Size = new Size(208, 23);
            lblNombreFinal.TabIndex = 5;
            lblNombreFinal.Text = "Esperando datos...";
            lblNombreFinal.Click += LblNombreFinal_Click;
            // 
            // btnCrearProyecto
            // 
            btnCrearProyecto.BackColor = Color.FromArgb(46, 125, 50);
            btnCrearProyecto.FlatAppearance.BorderSize = 0;
            btnCrearProyecto.FlatStyle = FlatStyle.Flat;
            btnCrearProyecto.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCrearProyecto.ForeColor = Color.White;
            btnCrearProyecto.Location = new Point(154, 380);
            btnCrearProyecto.Name = "btnCrearProyecto";
            btnCrearProyecto.Size = new Size(170, 40);
            btnCrearProyecto.TabIndex = 6;
            btnCrearProyecto.Text = "Crear Proyecto";
            btnCrearProyecto.UseVisualStyleBackColor = false;
            btnCrearProyecto.Click += BtnCrearProyecto_Click;
            btnCrearProyecto.MouseEnter += BtnCrearProyecto_MouseEnter;
            btnCrearProyecto.MouseLeave += BtnCrearProyecto_MouseLeave;
            // 
            // lblTitulo
            // 
            lblTitulo.BackColor = SystemColors.Control;
            lblTitulo.Font = new Font("Cooper Black", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTitulo.ForeColor = Color.DarkSlateGray;
            lblTitulo.Location = new Point(26, 17);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(427, 58);
            lblTitulo.TabIndex = 7;
            lblTitulo.Text = "Automatiza la creación de proyectos de C++";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            lblTitulo.Click += Label1_Click_2;
            // 
            // lblObjetivo
            // 
            lblObjetivo.AutoSize = true;
            lblObjetivo.BackColor = SystemColors.Control;
            lblObjetivo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblObjetivo.ForeColor = Color.DarkSlateGray;
            lblObjetivo.Location = new Point(141, 232);
            lblObjetivo.Name = "lblObjetivo";
            lblObjetivo.Size = new Size(197, 25);
            lblObjetivo.TabIndex = 8;
            lblObjetivo.Text = "Objetivo del ejercicio:";
            lblObjetivo.Click += LblObjetivo_Click;
            // 
            // txtObjetivo
            // 
            txtObjetivo.Location = new Point(114, 258);
            txtObjetivo.Multiline = true;
            txtObjetivo.Name = "txtObjetivo";
            txtObjetivo.ScrollBars = ScrollBars.Vertical;
            txtObjetivo.Size = new Size(250, 60);
            txtObjetivo.TabIndex = 9;
            txtObjetivo.TextChanged += TxtObjetivo_TextChanged;
            // 
            // pictureBoxfondo
            // 
            pictureBoxfondo.BackColor = Color.Transparent;
            pictureBoxfondo.Cursor = Cursors.Hand;
            pictureBoxfondo.Dock = DockStyle.Fill;
            pictureBoxfondo.Image = Properties.Resources.FondodeProyectoAutocpp;
            pictureBoxfondo.ImageLocation = "";
            pictureBoxfondo.Location = new Point(0, 0);
            pictureBoxfondo.Name = "pictureBoxfondo";
            pictureBoxfondo.Size = new Size(803, 535);
            pictureBoxfondo.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxfondo.TabIndex = 10;
            pictureBoxfondo.TabStop = false;
            pictureBoxfondo.Click += PictureBox1_Click;
            // 
            // frmPrincipal
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(803, 535);
            Controls.Add(lblTitulo);
            Controls.Add(lblObjetivo);
            Controls.Add(lblNombreFinal);
            Controls.Add(lblVistaPrevia);
            Controls.Add(lblNombre);
            Controls.Add(lblTema);
            Controls.Add(txtObjetivo);
            Controls.Add(btnCrearProyecto);
            Controls.Add(txtNombreProyecto);
            Controls.Add(cmbTemas);
            Controls.Add(pictureBoxfondo);
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "frmPrincipal";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ProjectCreator v1.0";
            Load += FrmPrincipal_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxfondo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTema;
        private ComboBox cmbTemas;
        private Label lblNombre;
        private TextBox txtNombreProyecto;
        private Label lblVistaPrevia;
        private Label lblNombreFinal;
        private Button btnCrearProyecto;
        private Label lblTitulo;
        private Label lblObjetivo;
        private TextBox txtObjetivo;
        private PictureBox pictureBoxfondo;
    }
}
