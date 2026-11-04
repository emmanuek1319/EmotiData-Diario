using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiarioInteligente
{
    public partial class PanelPsicologaForm : Form
    {
        private string idPsicologa;
        private static readonly HttpClient client = new HttpClient();

        public PanelPsicologaForm()
        {
            InitializeComponent();
            this.idPsicologa = "PSICO_DEMO";
        }

        public PanelPsicologaForm(string id)
        {
            InitializeComponent();
            this.idPsicologa = id;
        }

        private void PanelPsicologaForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 1. Cargar y Aplicar Tema inicial de la Psicóloga
            TemaManager.CargarTemaDeUsuario(this.idPsicologa);
            TemaManager.AplicarTema(this);

            ConfigurarTabla();

            // 2. CONECTAR EL EVENTO ANTES DE CARGAR LOS DATOS
            dgvEstudiantes.SelectionChanged -= DgvEstudiantes_SelectionChanged;
            dgvEstudiantes.SelectionChanged += DgvEstudiantes_SelectionChanged;

            // 3. Cargar la lista de alumnos
            CargarEstudiantes();
        }

        private void ConfigurarTabla()
        {
            dgvEstudiantes.Columns.Clear();
            dgvEstudiantes.Rows.Clear();

            dgvEstudiantes.Columns.Add("id", "ID");
            dgvEstudiantes.Columns.Add("nombre", "Nombre del Estudiante");
            dgvEstudiantes.Columns.Add("correo", "Correo");
            dgvEstudiantes.Columns.Add("ultima_emocion", "Última Emoción");
            dgvEstudiantes.Columns.Add("fecha_ultima", "Último Registro");
            dgvEstudiantes.Columns.Add("alerta", "Estado");

            dgvEstudiantes.Columns["id"].Visible = false;

            TemaManager.EstilarTabla(dgvEstudiantes);
        }

        private async void CargarEstudiantes()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:5000/api/psicologa/estudiantes");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    dgvEstudiantes.Rows.Clear();

                    if (root.TryGetProperty("estudiantes", out var estudiantesArray))
                    {
                        foreach (var est in estudiantesArray.EnumerateArray())
                        {
                            string id = est.TryGetProperty("id", out var pId) ? pId.GetString() : "";
                            string nombre = est.TryGetProperty("nombre", out var pNom) ? pNom.GetString() : "Sin Nombre";
                            string correo = est.TryGetProperty("correo", out var pCor) ? pCor.GetString() : "";
                            string emocion = est.TryGetProperty("ultima_emocion", out var pEmo) ? pEmo.GetString() : "--";
                            string fecha = est.TryGetProperty("fecha_ultima", out var pFec) ? pFec.GetString() : "--";
                            bool tieneAlerta = est.TryGetProperty("alerta", out var pAle) && pAle.GetBoolean();

                            string estadoTexto = tieneAlerta ? "⚠️ Atender" : "✅ Estable";

                            int rowIndex = dgvEstudiantes.Rows.Add(id, nombre, correo, emocion, fecha, estadoTexto);

                            if (tieneAlerta)
                            {
                                dgvEstudiantes.Rows[rowIndex].Cells["alerta"].Style.ForeColor = Color.IndianRed;
                                dgvEstudiantes.Rows[rowIndex].Cells["alerta"].Style.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                            }
                            else
                            {
                                dgvEstudiantes.Rows[rowIndex].Cells["alerta"].Style.ForeColor = Color.LightGreen;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la lista de estudiantes:\n{ex.Message}", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- CAMBIO DE TEMA EN TIEMPO REAL AL SELECCIONAR CUALQUIER FILA ---
        private void DgvEstudiantes_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEstudiantes.SelectedRows.Count > 0)
            {
                string idEstudiante = dgvEstudiantes.SelectedRows[0].Cells["id"].Value?.ToString() ?? "";

                if (!string.IsNullOrEmpty(idEstudiante))
                {
                    // Usa el propio gestor para cargar el tema directamente desde la DB usando el ID del alumno
                    TemaManager.CargarTemaDeUsuario(idEstudiante);
                    TemaManager.AplicarTema(this);
                    TemaManager.EstilarTabla(dgvEstudiantes);
                }
            }
        }

        private void btnVerExpediente_Click(object sender, EventArgs e)
        {
            if (dgvEstudiantes.SelectedRows.Count > 0)
            {
                string idEstudiante = dgvEstudiantes.SelectedRows[0].Cells["id"].Value?.ToString() ?? "";
                string nombreEstudiante = dgvEstudiantes.SelectedRows[0].Cells["nombre"].Value?.ToString() ?? "Estudiante";

                if (!string.IsNullOrEmpty(idEstudiante))
                {
                    // 1. Cargar tema del alumno
                    TemaManager.CargarTemaDeUsuario(idEstudiante);
                    TemaManager.AplicarTema(this);

                    // 2. Abrir expediente / estadísticas
                    EstadisticasForm expediente = new EstadisticasForm(idEstudiante);
                    expediente.ShowDialog();

                    // 3. Al cerrar, restaurar tema propio de la psicóloga
                    TemaManager.CargarTemaDeUsuario(this.idPsicologa);
                    TemaManager.AplicarTema(this);
                    TemaManager.EstilarTabla(dgvEstudiantes);
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un estudiante de la tabla.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnAgregarEstudiante_Click(object sender, EventArgs e)
        {
            AltaEstudianteForm formularioAlta = new AltaEstudianteForm();
            if (formularioAlta.ShowDialog() == DialogResult.OK)
            {
                CargarEstudiantes();
            }
        }

        private void btnCambiarTema_Click(object sender, EventArgs e)
        {
            TemaManager.SiguienteTema(this);
            TemaManager.EstilarTabla(dgvEstudiantes);
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Form1 login = new Form1();
            login.Show();
            this.Close();
        }
    }
}