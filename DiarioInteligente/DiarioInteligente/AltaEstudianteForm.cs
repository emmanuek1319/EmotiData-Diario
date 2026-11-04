using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace DiarioInteligente
{
    public class AltaEstudianteForm : Form
    {
        private TextBox txtNombre = null!;
        private TextBox txtCorreo = null!;
        private TextBox txtMatricula = null!;
        private TextBox txtCarrera = null!;
        private NumericUpDown numCuatrimestre = null!;
        private Button btnGuardar = null!;
        private Button btnCancelar = null!;
        private static readonly HttpClient client = new HttpClient();

        public AltaEstudianteForm()
        {
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            this.Text = "Dar de Alta Nuevo Estudiante";
            this.Size = new Size(420, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblTitulo = new Label { Text = "👤 Registro de Estudiante", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };

            Label lblNom = new Label { Text = "Nombre Completo:", Location = new Point(20, 55), AutoSize = true };
            txtNombre = new TextBox { Location = new Point(20, 75), Width = 360 };

            Label lblCor = new Label { Text = "Correo Institucional:", Location = new Point(20, 115), AutoSize = true };
            txtCorreo = new TextBox { Location = new Point(20, 135), Width = 360 };

            Label lblMat = new Label { Text = "Matrícula:", Location = new Point(20, 175), AutoSize = true };
            txtMatricula = new TextBox { Location = new Point(20, 195), Width = 360 };

            Label lblCar = new Label { Text = "Carrera:", Location = new Point(20, 235), AutoSize = true };
            txtCarrera = new TextBox { Location = new Point(20, 255), Width = 360, Text = "Tecnologías de la Información" };

            Label lblCuatri = new Label { Text = "Cuatrimestre:", Location = new Point(20, 295), AutoSize = true };
            numCuatrimestre = new NumericUpDown { Location = new Point(20, 315), Width = 120, Minimum = 1, Maximum = 12, Value = 1 };

            btnGuardar = new Button { Text = "💾 Registrar en MongoDB", Location = new Point(20, 370), Width = 220, Height = 40, DialogResult = DialogResult.None };
            btnCancelar = new Button { Text = "Cancelar", Location = new Point(250, 370), Width = 130, Height = 40, DialogResult = DialogResult.Cancel };

            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] {
                lblTitulo, lblNom, txtNombre, lblCor, txtCorreo, lblMat, txtMatricula,
                lblCar, txtCarrera, lblCuatri, numCuatrimestre, btnGuardar, btnCancelar
            });

            // Aplica el tema activo de la app
            TemaManager.AplicarTema(this);
        }

        private async void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtCorreo.Text) || string.IsNullOrWhiteSpace(txtMatricula.Text))
            {
                MessageBox.Show("Por favor, completa los campos requeridos (Nombre, Correo y Matrícula).", "Campos Faltantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnGuardar.Enabled = false;

                string matriculaLimpia = txtMatricula.Text.Trim();

                // Construcción exacta del objeto respetando la estructura de MongoDB
                var nuevoEstudiante = new
                {
                    _id = $"USR_{matriculaLimpia}",
                    nombre_completo = txtNombre.Text.Trim(),
                    correo = txtCorreo.Text.Trim(),
                    password = "123", // Contraseña inicial por defecto
                    rol = "estudiante",
                    detalles_estudiante = new
                    {
                        matricula = matriculaLimpia,
                        carrera = txtCarrera.Text.Trim(),
                        cuatrimestre = (int)numCuatrimestre.Value,
                        racha_dias = 0,
                        nivel = 1,
                        puntos_experiencia = 0
                    },
                    configuracion_interfaz = new
                    {
                        tema_color = "CafeCrema",
                        modo_oscuro = false
                    },
                    fecha_registro = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    activo = true
                };

                string jsonBody = JsonSerializer.Serialize(nuevoEstudiante);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/api/psicologa/registrar_estudiante", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("¡Estudiante dado de alta con éxito en MongoDB!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    string err = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error al guardar en el servidor:\n{err}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo conectar con el servidor:\n{ex.Message}", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnGuardar.Enabled = true;
            }
        }
    }
}