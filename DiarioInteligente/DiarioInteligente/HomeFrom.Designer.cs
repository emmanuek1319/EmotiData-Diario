using System.Drawing;
using System.Windows.Forms;

namespace DiarioInteligente
{
    partial class HomeFrom
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
            lblBienvenida = new Label();
            lblRacha = new Label();
            lblEmocion = new Label();
            lblFechaEmocion = new Label();
            btnNuevaEntrada = new Button();
            btnVerHistorial = new Button();
            btnCerrarSesion = new Button();
            grpResumen = new GroupBox();
            Estadisticasbton = new Button();
            lblProximoLogro = new Label();
            btnLogros = new Button();
            btnZonaRelax = new Button();
            btnCambiarTema = new Button();
            grpResumen.SuspendLayout();
            SuspendLayout();
            // 
            // lblBienvenida
            // 
            lblBienvenida.AutoSize = true;
            lblBienvenida.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblBienvenida.Location = new Point(30, 20);
            lblBienvenida.Name = "lblBienvenida";
            lblBienvenida.Size = new Size(258, 38);
            lblBienvenida.TabIndex = 0;
            lblBienvenida.Text = "¡Hola, bienvenido!";
            // 
            // lblRacha
            // 
            lblRacha.AutoSize = true;
            lblRacha.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblRacha.Location = new Point(17, 81);
            lblRacha.Name = "lblRacha";
            lblRacha.Size = new Size(253, 30);
            lblRacha.TabIndex = 0;
            lblRacha.Text = "🔥 Racha activa: 0 días";
            // 
            // lblEmocion
            // 
            lblEmocion.AutoSize = true;
            lblEmocion.Location = new Point(428, 86);
            lblEmocion.Name = "lblEmocion";
            lblEmocion.Size = new Size(180, 25);
            lblEmocion.TabIndex = 1;
            lblEmocion.Text = "Emoción: Cargando...";
            // 
            // lblFechaEmocion
            // 
            lblFechaEmocion.AutoSize = true;
            lblFechaEmocion.Location = new Point(849, 85);
            lblFechaEmocion.Name = "lblFechaEmocion";
            lblFechaEmocion.Size = new Size(80, 25);
            lblFechaEmocion.TabIndex = 2;
            lblFechaEmocion.Text = "Fecha: --";
            // 
            // btnNuevaEntrada
            // 
            btnNuevaEntrada.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNuevaEntrada.Location = new Point(30, 348);
            btnNuevaEntrada.Margin = new Padding(3, 4, 3, 4);
            btnNuevaEntrada.Name = "btnNuevaEntrada";
            btnNuevaEntrada.Size = new Size(220, 113);
            btnNuevaEntrada.TabIndex = 2;
            btnNuevaEntrada.Text = "✍️ Registrar Nueva Entrada";
            btnNuevaEntrada.UseVisualStyleBackColor = true;
            btnNuevaEntrada.Click += btnNuevaEntrada_Click;
            // 
            // btnVerHistorial
            // 
            btnVerHistorial.Font = new Font("Segoe UI", 10F);
            btnVerHistorial.Location = new Point(571, 348);
            btnVerHistorial.Margin = new Padding(3, 4, 3, 4);
            btnVerHistorial.Name = "btnVerHistorial";
            btnVerHistorial.Size = new Size(172, 113);
            btnVerHistorial.TabIndex = 3;
            btnVerHistorial.Text = "📜 Ver Historial";
            btnVerHistorial.UseVisualStyleBackColor = true;
            btnVerHistorial.Click += btnVerHistorial_Click;
            // 
            // btnCerrarSesion
            // 
            btnCerrarSesion.Location = new Point(1071, 537);
            btnCerrarSesion.Margin = new Padding(3, 4, 3, 4);
            btnCerrarSesion.Name = "btnCerrarSesion";
            btnCerrarSesion.Size = new Size(170, 50);
            btnCerrarSesion.TabIndex = 4;
            btnCerrarSesion.Text = "Cerrar Sesión";
            btnCerrarSesion.UseVisualStyleBackColor = true;
            btnCerrarSesion.Click += btnCerrarSesion_Click;
            // 
            // grpResumen
            // 
            grpResumen.Controls.Add(lblRacha);
            grpResumen.Controls.Add(lblEmocion);
            grpResumen.Controls.Add(lblFechaEmocion);
            grpResumen.Location = new Point(30, 113);
            grpResumen.Margin = new Padding(3, 4, 3, 4);
            grpResumen.Name = "grpResumen";
            grpResumen.Padding = new Padding(3, 4, 3, 4);
            grpResumen.Size = new Size(1211, 168);
            grpResumen.TabIndex = 1;
            grpResumen.TabStop = false;
            grpResumen.Text = "Resumen de Actividad";
            // 
            // Estadisticasbton
            // 
            Estadisticasbton.Location = new Point(795, 347);
            Estadisticasbton.Margin = new Padding(3, 4, 3, 4);
            Estadisticasbton.Name = "Estadisticasbton";
            Estadisticasbton.Size = new Size(186, 113);
            Estadisticasbton.TabIndex = 5;
            Estadisticasbton.Text = "📊 Estadisticas ";
            Estadisticasbton.UseVisualStyleBackColor = true;
            Estadisticasbton.Click += Estadisticasbton_Click;
            // 
            // lblProximoLogro
            // 
            lblProximoLogro.AutoSize = true;
            lblProximoLogro.Location = new Point(603, 65);
            lblProximoLogro.Name = "lblProximoLogro";
            lblProximoLogro.Size = new Size(252, 25);
            lblProximoLogro.TabIndex = 6;
            lblProximoLogro.Text = "🎯 Cargando próximo logro...";
            // 
            // btnLogros
            // 
            btnLogros.Location = new Point(1035, 347);
            btnLogros.Name = "btnLogros";
            btnLogros.Size = new Size(206, 114);
            btnLogros.TabIndex = 7;
            btnLogros.Text = "🏆 Mis Logros";
            btnLogros.UseVisualStyleBackColor = true;
            btnLogros.Click += btnLogros_Click;
            // 
            // btnZonaRelax
            // 
            btnZonaRelax.BackColor = Color.FromArgb(28, 34, 48);
            btnZonaRelax.FlatAppearance.BorderSize = 0;
            btnZonaRelax.FlatStyle = FlatStyle.Flat;
            btnZonaRelax.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnZonaRelax.ForeColor = Color.Gold;
            btnZonaRelax.Location = new Point(298, 347);
            btnZonaRelax.Name = "btnZonaRelax";
            btnZonaRelax.Size = new Size(220, 113);
            btnZonaRelax.TabIndex = 8;
            btnZonaRelax.Text = "\U0001f9d8‍♂️ Zona Relax";
            btnZonaRelax.UseVisualStyleBackColor = false;
            btnZonaRelax.Click += btnZonaRelax_Click;
            // 
            // btnCambiarTema
            // 
            btnCambiarTema.Location = new Point(1093, 13);
            btnCambiarTema.Name = "btnCambiarTema";
            btnCambiarTema.Size = new Size(148, 45);
            btnCambiarTema.TabIndex = 9;
            btnCambiarTema.Text = "🌙 / ☀️";
            btnCambiarTema.UseVisualStyleBackColor = true;
            btnCambiarTema.Click += btnCambiarTema_Click;
            // 
            // HomeFrom
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1253, 600);
            Controls.Add(btnCambiarTema);
            Controls.Add(btnZonaRelax);
            Controls.Add(btnLogros);
            Controls.Add(lblProximoLogro);
            Controls.Add(Estadisticasbton);
            Controls.Add(btnCerrarSesion);
            Controls.Add(btnVerHistorial);
            Controls.Add(btnNuevaEntrada);
            Controls.Add(grpResumen);
            Controls.Add(lblBienvenida);
            Margin = new Padding(3, 4, 3, 4);
            Name = "HomeFrom";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Menú Principal - Diario Inteligente";
            Load += HomeFrom_Load;
            grpResumen.ResumeLayout(false);
            grpResumen.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblBienvenida;
        private GroupBox grpResumen;
        private Label lblRacha;
        private Label lblEmocion;
        private Label lblFechaEmocion;
        private Button btnNuevaEntrada;
        private Button btnVerHistorial;
        private Button btnCerrarSesion;
        private Button Estadisticasbton;
        private Label lblProximoLogro;
        private Button btnLogros;
        private Button btnZonaRelax;
        private Button btnCambiarTema;
    }
}