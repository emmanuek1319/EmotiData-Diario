namespace DiarioInteligente
{
    partial class RegistroEmocionalForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitulo = new Label();
            txtEntrada = new TextBox();
            btnGuardar = new Button();
            grpResultado = new GroupBox();
            lblEmocionDetectada = new Label();
            txtRespuesta = new TextBox();
            btnVolver = new Button();
            grpResultado.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.Location = new Point(30, 20);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(365, 38);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "✍️ ¿Cómo te sentiste hoy?";
            // 
            // txtEntrada
            // 
            txtEntrada.Location = new Point(30, 70);
            txtEntrada.Multiline = true;
            txtEntrada.Name = "txtEntrada";
            txtEntrada.ScrollBars = ScrollBars.Vertical;
            txtEntrada.Size = new Size(720, 140);
            txtEntrada.TabIndex = 1;
            // 
            // btnGuardar
            // 
            btnGuardar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnGuardar.Location = new Point(30, 225);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(220, 45);
            btnGuardar.TabIndex = 2;
            btnGuardar.Text = "Analizar y Guardar";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // grpResultado
            // 
            grpResultado.Controls.Add(lblEmocionDetectada);
            grpResultado.Controls.Add(txtRespuesta);
            grpResultado.Location = new Point(30, 285);
            grpResultado.Name = "grpResultado";
            grpResultado.Size = new Size(720, 160);
            grpResultado.TabIndex = 3;
            grpResultado.TabStop = false;
            grpResultado.Text = "Respuesta del Diario Inteligente";
            // 
            // lblEmocionDetectada
            // 
            lblEmocionDetectada.AutoSize = true;
            lblEmocionDetectada.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblEmocionDetectada.Location = new Point(15, 30);
            lblEmocionDetectada.Name = "lblEmocionDetectada";
            lblEmocionDetectada.Size = new Size(217, 28);
            lblEmocionDetectada.TabIndex = 0;
            lblEmocionDetectada.Text = "Emoción: Esperando...";
            // 
            // txtRespuesta
            // 
            txtRespuesta.Location = new Point(15, 65);
            txtRespuesta.Multiline = true;
            txtRespuesta.Name = "txtRespuesta";
            txtRespuesta.ReadOnly = true;
            txtRespuesta.ScrollBars = ScrollBars.Vertical;
            txtRespuesta.Size = new Size(685, 80);
            txtRespuesta.TabIndex = 1;
            // 
            // btnVolver
            // 
            btnVolver.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnVolver.Location = new Point(580, 225);
            btnVolver.Name = "btnVolver";
            btnVolver.Size = new Size(170, 45);
            btnVolver.TabIndex = 4;
            btnVolver.Text = "Volver al Menú";
            btnVolver.UseVisualStyleBackColor = false;
            btnVolver.Click += btnVolver_Click;
            // 
            // RegistroEmocionalForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(780, 470);
            Controls.Add(btnVolver);
            Controls.Add(grpResultado);
            Controls.Add(btnGuardar);
            Controls.Add(txtEntrada);
            Controls.Add(lblTitulo);
            Name = "RegistroEmocionalForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Registro Emocional - Diario Inteligente";
            Load += RegistroEmocionalForm_Load; // <--- SE VINCULA EL EVENTO LOAD
            grpResultado.ResumeLayout(false);
            grpResultado.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private TextBox txtEntrada;
        private Button btnGuardar;
        private GroupBox grpResultado;
        private Label lblEmocionDetectada;
        private TextBox txtRespuesta;
        private Button btnVolver;
    }
}