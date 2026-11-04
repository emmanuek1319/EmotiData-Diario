using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace DiarioInteligente
{
    public partial class HistorialForm : Form
    {
        private string idUsuario;
        private HomeFrom menuPrincipal;
        private static readonly HttpClient client = new HttpClient();

        public HistorialForm(string id, HomeFrom home)
        {
            InitializeComponent();
            this.idUsuario = id;
            this.menuPrincipal = home;
        }

        private void HistorialForm_Load(object sender, EventArgs e)
        {
            // 1. FORZAR ESTADO NORMAL Y CENTRAR EN PANTALLA
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;

            // 2. BLOQUEO DEFINITIVO DE TAMAÑO (Mantiene proporciones de tu diseñador visual)
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;

            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            // 3. CARGAR Y APLICAR EL TEMA DEL USUARIO
            TemaManager.CargarTemaDeUsuario(this.idUsuario);
            TemaManager.AplicarTema(this);

            // 4. Cargar la lista de entradas de MongoDB
            CargarHistorial();
        }

        private async void CargarHistorial()
        {
            try
            {
                // RUTA EXACTA DE TU APP.PY
                HttpResponseMessage response = await client.GetAsync($"http://127.0.0.1:5000/api/diario/historial/{idUsuario}");

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonResponse);
                    var root = doc.RootElement;

                    // Busca tu DataGridView en la interfaz
                    DataGridView dgv = ObtenerDataGridView();

                    if (dgv != null)
                    {
                        PrepararColumnasTabla(dgv);
                        dgv.Rows.Clear();

                        // Tu backend envía la lista dentro de "entradas"
                        if (root.TryGetProperty("entradas", out var arrayEntradas) && arrayEntradas.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in arrayEntradas.EnumerateArray())
                            {
                                string fecha = item.TryGetProperty("fecha", out var f) ? f.GetString() : "--";
                                string emocion = item.TryGetProperty("emocion", out var e) ? e.GetString() : "Neutral";
                                string texto = item.TryGetProperty("texto", out var t) ? t.GetString() : "";

                                dgv.Rows.Add(fecha, emocion, texto);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("El servidor respondió con un error al consultar el historial.", "Error de Servidor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo obtener el historial desde Python:\n{ex.Message}", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrepararColumnasTabla(DataGridView dgv)
        {
            if (dgv.Columns.Count == 0)
            {
                dgv.Columns.Add("colFecha", "Fecha");
                dgv.Columns.Add("colEmocion", "Emoción");
                dgv.Columns.Add("colTexto", "Pensamiento / Entrada");

                dgv.Columns[0].Width = 140;
                dgv.Columns[1].Width = 120;
                dgv.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible = false;

            // Aplica los estilos y colores dinámicos del tema activo para la tabla
            TemaManager.EstilarTabla(dgv);
        }

        private DataGridView ObtenerDataGridView()
        {
            foreach (Control c in this.Controls)
            {
                if (c is DataGridView grid) return grid;

                foreach (Control sub in c.Controls)
                {
                    if (sub is DataGridView subGrid) return subGrid;
                }
            }
            return null;
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