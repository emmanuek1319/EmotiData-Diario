using System.Drawing;
using System.Windows.Forms;

namespace DiarioInteligente
{
    partial class HistorialForm
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
            dgvHistorial = new DataGridView();
            btnVolver = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).BeginInit();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.Location = new Point(27, 17);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(310, 36);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "📜 Historial de Entradas";
            // 
            // dgvHistorial
            // 
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
            dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistorial.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHistorial.Location = new Point(27, 63);
            dgvHistorial.Name = "dgvHistorial";
            dgvHistorial.ReadOnly = true;
            dgvHistorial.RowHeadersWidth = 51;
            dgvHistorial.Size = new Size(1303, 292);
            dgvHistorial.TabIndex = 1;
            // 
            // btnVolver
            // 
            btnVolver.Font = new Font("Segoe UI", 10F);
            btnVolver.Location = new Point(614, 451);
            btnVolver.Name = "btnVolver";
            btnVolver.Size = new Size(153, 34);
            btnVolver.TabIndex = 2;
            btnVolver.Text = "Volver al Menú";
            btnVolver.UseVisualStyleBackColor = true;
            btnVolver.Click += btnVolver_Click;
            // 
            // HistorialForm
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1342, 536);
            Controls.Add(btnVolver);
            Controls.Add(dgvHistorial);
            Controls.Add(lblTitulo);
            Name = "HistorialForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Historial de Entradas - Diario Inteligente";
            Load += HistorialForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvHistorial;
        private System.Windows.Forms.Button btnVolver;
    }
}