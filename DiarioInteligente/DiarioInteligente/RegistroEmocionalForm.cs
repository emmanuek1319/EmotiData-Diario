using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace DiarioInteligente
{
    public partial class RegistroEmocionalForm : Form
    {
        private string idUsuario;
        private HomeFrom menuPrincipal;
        private static readonly HttpClient client = new HttpClient();

        public RegistroEmocionalForm(string id, HomeFrom home)
        {
            InitializeComponent();
            this.idUsuario = id;
            this.menuPrincipal = home;
        }

        private void RegistroEmocionalForm_Load(object sender, EventArgs e)
        {
            // 1. Centrado y Bloqueo de Tamaño
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;

            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            // 2. CARGAR Y APLICAR TEMA INDIVIDUAL DEL USUARIO
            TemaManager.CargarTemaDeUsuario(this.idUsuario);
            TemaManager.AplicarTema(this);
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEntrada.Text))
            {
                MessageBox.Show("Por favor, escribe cómo te sientes antes de guardar.", "Campo Vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnGuardar.Enabled = false;

                var payload = new
                {
                    id_usuario = idUsuario,
                    texto = txtEntrada.Text.Trim()
                };

                string jsonBody = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/api/diario/guardar", content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonResponse);
                    var root = doc.RootElement;

                    string emocion = root.TryGetProperty("emocion", out var em) ? em.GetString() : "Registrada";
                    string mensajeEmpatico = root.TryGetProperty("mensaje_empatico", out var msg) ? msg.GetString() : "";

                    // Actualizar directamente por nombre de control
                    lblEmocionDetectada.Text = $"Emoción: {emocion}";
                    txtRespuesta.Text = mensajeEmpatico;

                    MessageBox.Show("¡Entrada analizada y guardada con éxito!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string errorDetalle = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error en el servidor Python:\n{errorDetalle}", "Error de Servidor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo conectar con el servidor Python.\n\nDetalle: {ex.Message}", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnGuardar.Enabled = true;
            }
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            if (menuPrincipal != null)
            {
                menuPrincipal.CargarResumenUsuario();
                menuPrincipal.Show();
            }
            this.Close();
        }
    }
}