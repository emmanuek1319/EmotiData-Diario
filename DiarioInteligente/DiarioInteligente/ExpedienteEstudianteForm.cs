using LiveCharts;
using LiveCharts.WinForms;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using WpfAxis = LiveCharts.Wpf.Axis;
using WpfSeparator = LiveCharts.Wpf.Separator;

namespace DiarioInteligente
{
    public partial class ExpedienteEstudianteForm : Form
    {
        private string idEstudiante;
        private CartesianChart chart;
        private static readonly HttpClient client = new HttpClient();

        public ExpedienteEstudianteForm(string id)
        {
            InitializeComponent();
            this.idEstudiante = id;
        }

        private void ExpedienteEstudianteForm_Load(object sender, EventArgs e)
        {
            // LÍNEA AGREGADA: Aplica el tema oscuro opaco al cargar la ventana
            TemaManager.AplicarTemaOscuro(this);

            // 1. Gráfica en la parte superior
            chart = new CartesianChart();
            chart.Location = new Point(20, 65);
            chart.Size = new Size(795, 230);
            this.Controls.Add(chart);

            // 2. Cargar datos
            CargarExpedienteUsuario();
            CargarGraficaEmocional();
            CargarAnalisisPsicologia();
        }

        private async void CargarExpedienteUsuario()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://127.0.0.1:5000/api/psicologa/expediente/{idEstudiante}");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    string nombre = root.GetProperty("nombre").GetString();
                    string correo = root.GetProperty("correo").GetString();
                    string matricula = root.GetProperty("matricula").GetString();
                    string carrera = root.GetProperty("carrera").GetString();
                    string cuatri = root.GetProperty("cuatrimestre").ToString();

                    lblDatosAlumno.Text = $"👤 {nombre}  |  Matrícula: {matricula}  |  Carrera: {carrera} ({cuatri}° Cuatrimestre)";
                    lblDatosAlumno.ForeColor = Color.DeepSkyBlue; // Color vistoso en fondo oscuro

                    txtNotasClinicas.Text = root.GetProperty("notas_clinicas").GetString();
                }
            }
            catch (Exception ex)
            {
                lblDatosAlumno.Text = "Error al cargar datos del estudiante.";
            }
        }

        private void CargarGraficaEmocional()
        {
            try
            {
                var clientMongo = new MongoClient("mongodb://localhost:27017");
                var database = clientMongo.GetDatabase("diario_inteligente");
                var collection = database.GetCollection<BsonDocument>("diario_logs");

                var builder = Builders<BsonDocument>.Filter;
                var filtroUsuario = builder.Or(
                    builder.Eq("usuario_id", idEstudiante),
                    builder.Eq("id_usuario", idEstudiante)
                );

                var registros = collection.Find(filtroUsuario)
                                          .Sort(Builders<BsonDocument>.Sort.Descending("fecha"))
                                          .Limit(10)
                                          .ToList();

                registros.Reverse();

                ChartValues<double> valoresPuntuacion = new ChartValues<double>();
                List<string> fechasEtiquetas = new List<string>();

                foreach (var doc in registros)
                {
                    double puntuacion = 0;
                    if (doc.Contains("puntuacion") && !doc["puntuacion"].IsBsonNull)
                    {
                        puntuacion = Math.Round(Convert.ToDouble(doc["puntuacion"].RawValue), 2);
                    }

                    string fechaCorta = "Sin fecha";
                    if (doc.Contains("fecha") && !doc["fecha"].IsBsonNull)
                    {
                        string fechaCompleta = doc["fecha"].AsString;
                        if (DateTime.TryParse(fechaCompleta, out DateTime dt))
                        {
                            fechaCorta = dt.ToString("dd/MMM");
                        }
                        else
                        {
                            fechaCorta = fechaCompleta;
                        }
                    }

                    valoresPuntuacion.Add(puntuacion);
                    fechasEtiquetas.Add(fechaCorta);
                }

                var lineaAnimo = new LiveCharts.Wpf.LineSeries
                {
                    Title = "Ánimo del Alumno",
                    Values = valoresPuntuacion,
                    PointGeometrySize = 10,
                    LineSmoothness = 0.5,
                    DataLabels = true,
                    Stroke = System.Windows.Media.Brushes.DeepSkyBlue,
                    Fill = System.Windows.Media.Brushes.Transparent
                };

                chart.Series = new SeriesCollection { lineaAnimo };

                chart.AxisX.Clear();
                chart.AxisX.Add(new WpfAxis
                {
                    Title = "Fecha",
                    Labels = fechasEtiquetas.ToArray(),
                    Foreground = System.Windows.Media.Brushes.White,
                    Separator = new WpfSeparator { Step = 1, IsEnabled = false }
                });

                chart.AxisY.Clear();
                chart.AxisY.Add(new WpfAxis
                {
                    Title = "Estado (-1 Negativo | +1 Positivo)",
                    MinValue = -1.0,
                    MaxValue = 1.0,
                    Foreground = System.Windows.Media.Brushes.White,
                    Separator = new WpfSeparator { Step = 0.5 }
                });
            }
            catch { }
        }

        private async void CargarAnalisisPsicologia()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://127.0.0.1:5000/api/diario/reporte_psicologica/{idEstudiante}");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("puntos_ciegos", out var pcElem))
                    {
                        lblPuntosCiegos.Text = $"PUNTOS CIEGOS DETECTADOS:\n{pcElem.GetString()}";
                        lblPuntosCiegos.ForeColor = Color.LightGray;
                    }
                    if (root.TryGetProperty("recomendacion", out var recElem))
                    {
                        lblRecomendacion.Text = recElem.GetString();
                        lblRecomendacion.ForeColor = Color.Gold;
                    }
                }
            }
            catch
            {
                lblPuntosCiegos.Text = "Sin análisis disponible.";
            }
        }

        private async void btnGuardarNotas_Click(object sender, EventArgs e)
        {
            try
            {
                var body = new { id_estudiante = idEstudiante, notas = txtNotasClinicas.Text.Trim() };
                string jsonBody = JsonSerializer.Serialize(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/api/psicologa/guardar_notas", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("¡Notas de seguimiento guardadas correctamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar notas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}