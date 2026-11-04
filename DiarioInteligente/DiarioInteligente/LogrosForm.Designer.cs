namespace DiarioInteligente
{
    partial class LogrosForm
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            lblTituloPantalla = new Label();
            lblResumen = new Label();
            lblProximoReto = new Label();
            panelMedallas = new FlowLayoutPanel();
            btnVolver = new Button();
            SuspendLayout();
            // 
            // lblTituloPantalla
            // 
            lblTituloPantalla.AutoSize = true;
            lblTituloPantalla.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTituloPantalla.ForeColor = Color.FromArgb(30, 30, 30);
            lblTituloPantalla.Location = new Point(29, 25);
            lblTituloPantalla.Margin = new Padding(4, 0, 4, 0);
            lblTituloPantalla.Name = "lblTituloPantalla";
            lblTituloPantalla.Size = new Size(409, 45);
            lblTituloPantalla.TabIndex = 0;
            lblTituloPantalla.Text = "🏆 Mis Logros y Medallas";
            // 
            // lblResumen
            // 
            lblResumen.AutoSize = true;
            lblResumen.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblResumen.ForeColor = Color.FromArgb(0, 102, 204);
            lblResumen.Location = new Point(31, 87);
            lblResumen.Margin = new Padding(4, 0, 4, 0);
            lblResumen.Name = "lblResumen";
            lblResumen.Size = new Size(450, 30);
            lblResumen.TabIndex = 1;
            lblResumen.Text = "🔥 Racha: Cargando... | 🏅 Medallas: --/--";
            // 
            // lblProximoReto
            // 
            lblProximoReto.AutoSize = true;
            lblProximoReto.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
            lblProximoReto.ForeColor = Color.DarkSlateGray;
            lblProximoReto.Location = new Point(31, 130);
            lblProximoReto.Margin = new Padding(4, 0, 4, 0);
            lblProximoReto.Name = "lblProximoReto";
            lblProximoReto.Size = new Size(334, 28);
            lblProximoReto.TabIndex = 2;
            lblProximoReto.Text = "🎯 Próximo logro: Cargando metas...";
            // 
            // panelMedallas
            // 
            panelMedallas.AutoScroll = true;
            panelMedallas.BackColor = Color.WhiteSmoke;
            panelMedallas.BorderStyle = BorderStyle.FixedSingle;
            panelMedallas.Location = new Point(13, 163);
            panelMedallas.Margin = new Padding(4, 5, 4, 5);
            panelMedallas.Name = "panelMedallas";
            panelMedallas.Padding = new Padding(11, 13, 11, 13);
            panelMedallas.Size = new Size(1189, 719);
            panelMedallas.TabIndex = 3;
            // 
            // btnVolver
            // 
            btnVolver.BackColor = Color.LightGray;
            btnVolver.FlatAppearance.BorderSize = 0;
            btnVolver.FlatStyle = FlatStyle.Flat;
            btnVolver.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnVolver.Location = new Point(1003, 908);
            btnVolver.Margin = new Padding(4, 5, 4, 5);
            btnVolver.Name = "btnVolver";
            btnVolver.Size = new Size(200, 63);
            btnVolver.TabIndex = 4;
            btnVolver.Text = "Volver al Menú";
            btnVolver.UseVisualStyleBackColor = false;
            btnVolver.Click += btnVolver_Click;
            // 
            // LogrosForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 249, 250);
            ClientSize = new Size(1211, 974);
            Controls.Add(btnVolver);
            Controls.Add(panelMedallas);
            Controls.Add(lblProximoReto);
            Controls.Add(lblResumen);
            Controls.Add(lblTituloPantalla);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            Name = "LogrosForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Mis Logros - Diario Inteligente";
            Load += LogrosForm_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTituloPantalla;
        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.Label lblProximoReto;
        private System.Windows.Forms.FlowLayoutPanel panelMedallas;
        private System.Windows.Forms.Button btnVolver;
    }
}