namespace DiarioInteligente
{
    partial class PanelPsicologaForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitulo = new Label();
            lblSubtitulo = new Label();
            dgvEstudiantes = new DataGridView();
            btnVerExpediente = new Button();
            btnCerrarSesion = new Button();
            btnAgregarEstudiante = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvEstudiantes).BeginInit();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(30, 30, 30);
            lblTitulo.Location = new Point(193, 24);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(476, 45);
            lblTitulo.TabIndex = 4;
            lblTitulo.Text = "👩‍⚕️ Panel de Acompañamiento";
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Font = new Font("Segoe UI", 10F);
            lblSubtitulo.ForeColor = Color.DimGray;
            lblSubtitulo.Location = new Point(111, 96);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Size = new Size(647, 28);
            lblSubtitulo.TabIndex = 3;
            lblSubtitulo.Text = "Selecciona un estudiante para revisar su expediente emocional completo.";
            // 
            // dgvEstudiantes
            // 
            dgvEstudiantes.AllowUserToAddRows = false;
            dgvEstudiantes.AllowUserToDeleteRows = false;
            dgvEstudiantes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvEstudiantes.BackgroundColor = Color.White;
            dgvEstudiantes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEstudiantes.Location = new Point(74, 175);
            dgvEstudiantes.MultiSelect = false;
            dgvEstudiantes.Name = "dgvEstudiantes";
            dgvEstudiantes.ReadOnly = true;
            dgvEstudiantes.RowHeadersWidth = 62;
            dgvEstudiantes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEstudiantes.Size = new Size(780, 380);
            dgvEstudiantes.TabIndex = 2;
            // 
            // btnVerExpediente
            // 
            btnVerExpediente.BackColor = Color.FromArgb(0, 122, 204);
            btnVerExpediente.FlatAppearance.BorderSize = 0;
            btnVerExpediente.FlatStyle = FlatStyle.Flat;
            btnVerExpediente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnVerExpediente.ForeColor = Color.White;
            btnVerExpediente.Location = new Point(22, 576);
            btnVerExpediente.Name = "btnVerExpediente";
            btnVerExpediente.Size = new Size(200, 40);
            btnVerExpediente.TabIndex = 1;
            btnVerExpediente.Text = "📋 Ver Expediente";
            btnVerExpediente.UseVisualStyleBackColor = false;
            btnVerExpediente.Click += btnVerExpediente_Click;
            // 
            // btnCerrarSesion
            // 
            btnCerrarSesion.BackColor = Color.LightGray;
            btnCerrarSesion.FlatAppearance.BorderSize = 0;
            btnCerrarSesion.FlatStyle = FlatStyle.Flat;
            btnCerrarSesion.Font = new Font("Segoe UI", 10F);
            btnCerrarSesion.Location = new Point(770, 576);
            btnCerrarSesion.Name = "btnCerrarSesion";
            btnCerrarSesion.Size = new Size(140, 40);
            btnCerrarSesion.TabIndex = 0;
            btnCerrarSesion.Text = "Cerrar Sesión";
            btnCerrarSesion.UseVisualStyleBackColor = false;
            btnCerrarSesion.Click += btnCerrarSesion_Click;
            // 
            // btnAgregarEstudiante
            // 
            btnAgregarEstudiante.Location = new Point(741, 51);
            btnAgregarEstudiante.Name = "btnAgregarEstudiante";
            btnAgregarEstudiante.Size = new Size(151, 34);
            btnAgregarEstudiante.TabIndex = 5;
            btnAgregarEstudiante.Text = "➕ Registrar Estudiante";
            btnAgregarEstudiante.UseVisualStyleBackColor = true;
            btnAgregarEstudiante.Click += btnAgregarEstudiante_Click;
            // 
            // PanelPsicologaForm
            // 
            ClientSize = new Size(922, 643);
            Controls.Add(btnAgregarEstudiante);
            Controls.Add(btnCerrarSesion);
            Controls.Add(btnVerExpediente);
            Controls.Add(dgvEstudiantes);
            Controls.Add(lblSubtitulo);
            Controls.Add(lblTitulo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "PanelPsicologaForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Módulo Especialista - Diario Inteligente";
            Load += PanelPsicologaForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvEstudiantes).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.DataGridView dgvEstudiantes;
        private System.Windows.Forms.Button btnVerExpediente;
        private System.Windows.Forms.Button btnCerrarSesion;
        private Button btnAgregarEstudiante;
    }
}