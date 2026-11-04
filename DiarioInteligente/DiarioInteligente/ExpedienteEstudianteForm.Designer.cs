namespace DiarioInteligente
{
    partial class ExpedienteEstudianteForm
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
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblDatosAlumno = new System.Windows.Forms.Label();
            this.groupObservaciones = new System.Windows.Forms.GroupBox();
            this.lblPuntosCiegos = new System.Windows.Forms.Label();
            this.lblRecomendacion = new System.Windows.Forms.Label();
            this.groupNotas = new System.Windows.Forms.GroupBox();
            this.txtNotasClinicas = new System.Windows.Forms.TextBox();
            this.btnGuardarNotas = new System.Windows.Forms.Button();
            this.btnVolver = new System.Windows.Forms.Button();
            this.groupObservaciones.SuspendLayout();
            this.groupNotas.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblTitulo.Location = new System.Drawing.Point(20, 12);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(310, 25);
            this.lblTitulo.Text = "📁 Expediente Emocional del Alumno";
            // 
            // lblDatosAlumno
            // 
            this.lblDatosAlumno.AutoSize = true;
            this.lblDatosAlumno.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.lblDatosAlumno.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.lblDatosAlumno.Location = new System.Drawing.Point(22, 42);
            this.lblDatosAlumno.Name = "lblDatosAlumno";
            this.lblDatosAlumno.Size = new System.Drawing.Size(250, 17);
            this.lblDatosAlumno.Text = "Cargando información del estudiante...";
            // 
            // groupObservaciones
            // 
            this.groupObservaciones.Controls.Add(this.lblRecomendacion);
            this.groupObservaciones.Controls.Add(this.lblPuntosCiegos);
            this.groupObservaciones.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.groupObservaciones.Location = new System.Drawing.Point(20, 310);
            this.groupObservaciones.Name = "groupObservaciones";
            this.groupObservaciones.Size = new System.Drawing.Size(480, 210);
            this.groupObservaciones.TabIndex = 1;
            this.groupObservaciones.TabStop = false;
            this.groupObservaciones.Text = "🔍 Puntos Ciegos y Patrones Emocionales";
            // 
            // lblPuntosCiegos
            // 
            this.lblPuntosCiegos.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblPuntosCiegos.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblPuntosCiegos.Location = new System.Drawing.Point(12, 22);
            this.lblPuntosCiegos.Name = "lblPuntosCiegos";
            this.lblPuntosCiegos.Size = new System.Drawing.Size(455, 100);
            this.lblPuntosCiegos.Text = "Cargando puntos ciegos...";
            // 
            // lblRecomendacion
            // 
            this.lblRecomendacion.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Italic);
            this.lblRecomendacion.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblRecomendacion.Location = new System.Drawing.Point(12, 130);
            this.lblRecomendacion.Name = "lblRecomendacion";
            this.lblRecomendacion.Size = new System.Drawing.Size(455, 70);
            this.lblRecomendacion.Text = "Cargando sugerencia...";
            // 
            // groupNotas
            // 
            this.groupNotas.Controls.Add(this.btnGuardarNotas);
            this.groupNotas.Controls.Add(this.txtNotasClinicas);
            this.groupNotas.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.groupNotas.Location = new System.Drawing.Point(515, 310);
            this.groupNotas.Name = "groupNotas";
            this.groupNotas.Size = new System.Drawing.Size(300, 210);
            this.groupNotas.TabIndex = 2;
            this.groupNotas.TabStop = false;
            this.groupNotas.Text = "📝 Notas Privadas de Seguimiento";
            // 
            // txtNotasClinicas
            // 
            this.txtNotasClinicas.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            this.txtNotasClinicas.Location = new System.Drawing.Point(12, 22);
            this.txtNotasClinicas.Multiline = true;
            this.txtNotasClinicas.Name = "txtNotasClinicas";
            this.txtNotasClinicas.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtNotasClinicas.Size = new System.Drawing.Size(275, 135);
            // 
            // btnGuardarNotas
            // 
            this.btnGuardarNotas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnGuardarNotas.FlatAppearance.BorderSize = 0;
            this.btnGuardarNotas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarNotas.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.btnGuardarNotas.ForeColor = System.Drawing.Color.White;
            this.btnGuardarNotas.Location = new System.Drawing.Point(12, 165);
            this.btnGuardarNotas.Name = "btnGuardarNotas";
            this.btnGuardarNotas.Size = new System.Drawing.Size(275, 32);
            this.btnGuardarNotas.Text = "💾 Guardar Observaciones";
            this.btnGuardarNotas.UseVisualStyleBackColor = false;
            this.btnGuardarNotas.Click += new System.EventHandler(this.btnGuardarNotas_Click);
            // 
            // btnVolver
            // 
            this.btnVolver.BackColor = System.Drawing.Color.LightGray;
            this.btnVolver.FlatAppearance.BorderSize = 0;
            this.btnVolver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVolver.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.btnVolver.Location = new System.Drawing.Point(705, 12);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(110, 32);
            this.btnVolver.Text = " Volver";
            this.btnVolver.UseVisualStyleBackColor = false;
            this.btnVolver.Click += new System.EventHandler(this.btnVolver_Click);
            // 
            // ExpedienteEstudianteForm
            // 
            this.ClientSize = new System.Drawing.Size(834, 531);
            this.Controls.Add(this.btnVolver);
            this.Controls.Add(this.groupNotas);
            this.Controls.Add(this.groupObservaciones);
            this.Controls.Add(this.lblDatosAlumno);
            this.Controls.Add(this.lblTitulo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ExpedienteEstudianteForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Expediente Clínico del Estudiante";
            this.Load += new System.EventHandler(this.ExpedienteEstudianteForm_Load);
            this.groupObservaciones.ResumeLayout(false);
            this.groupNotas.ResumeLayout(false);
            this.groupNotas.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblDatosAlumno;
        private System.Windows.Forms.GroupBox groupObservaciones;
        private System.Windows.Forms.Label lblPuntosCiegos;
        private System.Windows.Forms.Label lblRecomendacion;
        private System.Windows.Forms.GroupBox groupNotas;
        private System.Windows.Forms.TextBox txtNotasClinicas;
        private System.Windows.Forms.Button btnGuardarNotas;
        private System.Windows.Forms.Button btnVolver;
    }
}