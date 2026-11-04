namespace DiarioInteligente
{
    partial class ZonaRelaxForm
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
            components = new System.ComponentModel.Container();
            lblTitulo = new Label();
            pnlFrases = new Panel();
            btnSiguienteFrase = new Button();
            lblFraseAutor = new Label();
            lblFraseTexto = new Label();
            timerFrases = new System.Windows.Forms.Timer(components);
            pnlRespiracion = new GroupBox();
            btnIniciarRespiracion = new Button();
            lblEstadoRespiracion = new Label();
            lblCirculoRespiracion = new Label();
            timerRespiracion = new System.Windows.Forms.Timer(components);
            btnVolver = new Button();
            lnkLineaVida = new LinkLabel();
            pnlFrases.SuspendLayout();
            pnlRespiracion.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.Gold;
            lblTitulo.Location = new Point(68, 9);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(391, 45);
            lblTitulo.TabIndex = 2;
            lblTitulo.Text = "\U0001f9d8‍♂️ Zona Confort y Relax";
            // 
            // pnlFrases
            // 
            pnlFrases.BackColor = Color.FromArgb(28, 34, 48);
            pnlFrases.Controls.Add(btnSiguienteFrase);
            pnlFrases.Controls.Add(lblFraseAutor);
            pnlFrases.Controls.Add(lblFraseTexto);
            pnlFrases.Location = new Point(68, 63);
            pnlFrases.Name = "pnlFrases";
            pnlFrases.Size = new Size(834, 241);
            pnlFrases.TabIndex = 0;
            // 
            // btnSiguienteFrase
            // 
            btnSiguienteFrase.FlatAppearance.BorderSize = 0;
            btnSiguienteFrase.FlatStyle = FlatStyle.Flat;
            btnSiguienteFrase.Font = new Font("Segoe UI", 9F);
            btnSiguienteFrase.ForeColor = Color.LightGray;
            btnSiguienteFrase.Location = new Point(320, 148);
            btnSiguienteFrase.Name = "btnSiguienteFrase";
            btnSiguienteFrase.Size = new Size(152, 55);
            btnSiguienteFrase.TabIndex = 0;
            btnSiguienteFrase.Text = "✨ Otra frase";
            btnSiguienteFrase.UseVisualStyleBackColor = true;
            btnSiguienteFrase.Click += btnSiguienteFrase_Click;
            // 
            // lblFraseAutor
            // 
            lblFraseAutor.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFraseAutor.ForeColor = Color.Gold;
            lblFraseAutor.Location = new Point(20, 120);
            lblFraseAutor.Name = "lblFraseAutor";
            lblFraseAutor.Size = new Size(730, 25);
            lblFraseAutor.TabIndex = 1;
            lblFraseAutor.Text = "- Diario Inteligente";
            lblFraseAutor.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblFraseTexto
            // 
            lblFraseTexto.Font = new Font("Georgia", 14F, FontStyle.Italic);
            lblFraseTexto.ForeColor = Color.WhiteSmoke;
            lblFraseTexto.Location = new Point(20, 25);
            lblFraseTexto.Name = "lblFraseTexto";
            lblFraseTexto.Size = new Size(730, 90);
            lblFraseTexto.TabIndex = 2;
            lblFraseTexto.Text = "\"Cargando frase inspiradora desde el servidor...\"";
            lblFraseTexto.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // timerFrases
            // 
            timerFrases.Interval = 7000;
            timerFrases.Tick += timerFrases_Tick;
            // 
            // pnlRespiracion
            // 
            pnlRespiracion.Controls.Add(btnIniciarRespiracion);
            pnlRespiracion.Controls.Add(lblEstadoRespiracion);
            pnlRespiracion.Controls.Add(lblCirculoRespiracion);
            pnlRespiracion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            pnlRespiracion.ForeColor = Color.White;
            pnlRespiracion.Location = new Point(89, 337);
            pnlRespiracion.Name = "pnlRespiracion";
            pnlRespiracion.Size = new Size(770, 220);
            pnlRespiracion.TabIndex = 1;
            pnlRespiracion.TabStop = false;
            pnlRespiracion.Text = "\U0001fac1 Ejercicio de Respiración Guiada (4-7-8)";
            // 
            // btnIniciarRespiracion
            // 
            btnIniciarRespiracion.BackColor = Color.LightSeaGreen;
            btnIniciarRespiracion.FlatAppearance.BorderSize = 0;
            btnIniciarRespiracion.FlatStyle = FlatStyle.Flat;
            btnIniciarRespiracion.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnIniciarRespiracion.ForeColor = Color.White;
            btnIniciarRespiracion.Location = new Point(310, 172);
            btnIniciarRespiracion.Name = "btnIniciarRespiracion";
            btnIniciarRespiracion.Size = new Size(150, 35);
            btnIniciarRespiracion.TabIndex = 0;
            btnIniciarRespiracion.Text = "▶️ Iniciar Guiado";
            btnIniciarRespiracion.UseVisualStyleBackColor = false;
            btnIniciarRespiracion.Click += btnIniciarRespiracion_Click;
            // 
            // lblEstadoRespiracion
            // 
            lblEstadoRespiracion.Font = new Font("Segoe UI", 11F);
            lblEstadoRespiracion.ForeColor = Color.Khaki;
            lblEstadoRespiracion.Location = new Point(20, 135);
            lblEstadoRespiracion.Name = "lblEstadoRespiracion";
            lblEstadoRespiracion.Size = new Size(730, 30);
            lblEstadoRespiracion.TabIndex = 1;
            lblEstadoRespiracion.Text = "Presiona iniciar para regular tu ritmo cardíaco y calma mental.";
            lblEstadoRespiracion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCirculoRespiracion
            // 
            lblCirculoRespiracion.BackColor = Color.LightSeaGreen;
            lblCirculoRespiracion.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblCirculoRespiracion.ForeColor = Color.White;
            lblCirculoRespiracion.Location = new Point(340, 35);
            lblCirculoRespiracion.Name = "lblCirculoRespiracion";
            lblCirculoRespiracion.Size = new Size(90, 90);
            lblCirculoRespiracion.TabIndex = 0;
            lblCirculoRespiracion.Text = "4";
            lblCirculoRespiracion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // timerRespiracion
            // 
            timerRespiracion.Interval = 1000;
            timerRespiracion.Tick += timerRespiracion_Tick;
            // 
            // btnVolver
            // 
            btnVolver.BackColor = Color.FromArgb(45, 55, 72);
            btnVolver.FlatAppearance.BorderSize = 0;
            btnVolver.FlatStyle = FlatStyle.Flat;
            btnVolver.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnVolver.ForeColor = Color.White;
            btnVolver.Location = new Point(792, 9);
            btnVolver.Name = "btnVolver";
            btnVolver.Size = new Size(110, 32);
            btnVolver.TabIndex = 0;
            btnVolver.Text = " Volver";
            btnVolver.UseVisualStyleBackColor = false;
            btnVolver.Click += btnVolver_Click;
            // 
            // lnkLineaVida
            // 
            lnkLineaVida.AutoSize = true;
            lnkLineaVida.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lnkLineaVida.Location = new Point(246, 309);
            lnkLineaVida.Name = "lnkLineaVida";
            lnkLineaVida.Size = new Size(495, 28);
            lnkLineaVida.TabIndex = 3;
            lnkLineaVida.TabStop = true;
            lnkLineaVida.Text = "📞 Línea de la Vida: 800 911 2000 (Atención 24/7)";
            lnkLineaVida.LinkClicked += linkLabel1_LinkClicked;
            // 
            // ZonaRelaxForm
            // 
            BackColor = Color.FromArgb(18, 22, 31);
            ClientSize = new Size(967, 569);
            Controls.Add(lnkLineaVida);
            Controls.Add(btnVolver);
            Controls.Add(pnlRespiracion);
            Controls.Add(pnlFrases);
            Controls.Add(lblTitulo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "ZonaRelaxForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Diario Inteligente - Espacio Confort & Calma";
            Load += ZonaRelaxForm_Load;
            pnlFrases.ResumeLayout(false);
            pnlRespiracion.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlFrases;
        private System.Windows.Forms.Label lblFraseTexto;
        private System.Windows.Forms.Label lblFraseAutor;
        private System.Windows.Forms.Button btnSiguienteFrase;
        private System.Windows.Forms.Timer timerFrases;
        private System.Windows.Forms.GroupBox pnlRespiracion;
        private System.Windows.Forms.Label lblCirculoRespiracion;
        private System.Windows.Forms.Label lblEstadoRespiracion;
        private System.Windows.Forms.Button btnIniciarRespiracion;
        private System.Windows.Forms.Timer timerRespiracion;
        private System.Windows.Forms.Button btnVolver;
        private LinkLabel lnkLineaVida;
    }
}