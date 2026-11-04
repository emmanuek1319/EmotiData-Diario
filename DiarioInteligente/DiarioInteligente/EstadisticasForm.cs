using LiveCharts;
using LiveCharts.WinForms;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using WpfAxis = LiveCharts.Wpf.Axis;
using WpfSeparator = LiveCharts.Wpf.Separator;

namespace DiarioInteligente
{
    public partial class EstadisticasForm : Form
    {
        private CartesianChart chart;
        private string idUsuario;
        private Label lblPuntosCiegos;
        private Label lblRecomendacion;
        private Button btnExportar;
        private GroupBox groupReporte;
        private static readonly HttpClient client = new HttpClient();

        public EstadisticasForm()
        {
            InitializeComponent();
            this.idUsuario = "USR_DEMO";
        }

        public EstadisticasForm(string id)
        {
            InitializeComponent();
            this.idUsuario = id;
        }

        private async void EstadisticasForm_Load(object sender, EventArgs e)
        {
            this.ClientSize = new Size(820, 530);
            this.Text = "Estadísticas y Reporte Psicológico - Diario Inteligente";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 1. CARGAR Y APLICAR EL TEMA ESPECÍFICO DEL ALUMNO CONSULTADO
            await CargarTemaDelAlumno(this.idUsuario);
            TemaManager.AplicarTema(this);

            // 2. Gráfica de LiveCharts
            chart = new CartesianChart();
            chart.Location = new Point(15, 10);
            chart.Size = new Size(790, 250);
            this.Controls.Add(chart);

            // 3. Contenedor del Reporte para la Psicóloga
            groupReporte = new GroupBox
            {
                Text = "🔍 Observaciones Clínicas y Puntos Ciegos (Para Psicología)",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(15, 270),
                Size = new Size(620, 240)
            };

            lblPuntosCiegos = new Label
            {
                Text = "Cargando análisis de puntos ciegos...",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Location = new Point(12, 22),
                Size = new Size(595, 115)
            };

            lblRecomendacion = new Label
            {
                Text = "Cargando sugerencia...",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Location = new Point(12, 145),
                Size = new Size(595, 85)
            };

            groupReporte.Controls.Add(lblPuntosCiegos);
            groupReporte.Controls.Add(lblRecomendacion);
            this.Controls.Add(groupReporte);

            // 4. Botón Exportar
            btnExportar = new Button
            {
                Text = "📄 Guardar Reporte\n(Imagen HD)",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(650, 350),
                Size = new Size(155, 55)
            };
            btnExportar.FlatAppearance.BorderSize = 1;
            btnExportar.Click += BtnExportar_Click;
            this.Controls.Add(btnExportar);

            TemaManager.AplicarTema(this);

            CargarDatosEmociones();
            CargarAnalisisPsicologia();
        }

        private async Task CargarTemaDelAlumno(string id)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://127.0.0.1:5000/api/usuarios/configuracion/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    string temaEstudiante = "";
                    if (root.TryGetProperty("configuracion_interfaz", out var configObj) && configObj.TryGetProperty("tema", out var tElem))
                    {
                        temaEstudiante = tElem.GetString() ?? "";
                    }
                    else if (root.TryGetProperty("tema", out var propTema))
                    {
                        temaEstudiante = propTema.GetString() ?? "";
                    }

                    if (!string.IsNullOrEmpty(temaEstudiante))
                    {
                        Array valoresEnum = Enum.GetValues(typeof(TipoTema));
                        foreach (TipoTema item in valoresEnum)
                        {
                            if (item.ToString().Equals(temaEstudiante, StringComparison.OrdinalIgnoreCase) ||
                                temaEstudiante.IndexOf(item.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                TemaManager.EstablecerTema(item);
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Manejo silencioso en fallback
            }
        }

        private void CargarDatosEmociones()
        {
            try
            {
                var clientMongo = new MongoClient("mongodb://localhost:27017");
                var database = clientMongo.GetDatabase("diario_inteligente");
                var collection = database.GetCollection<BsonDocument>("diario_logs");

                var builder = Builders<BsonDocument>.Filter;
                var filtroUsuario = builder.Or(
                    builder.Eq("usuario_id", idUsuario),
                    builder.Eq("id_usuario", idUsuario)
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

                System.Windows.Media.Brush colorLinea = TemaManager.EsModoOscuro
                    ? System.Windows.Media.Brushes.Cyan
                    : System.Windows.Media.Brushes.MediumVioletRed; // Detalle rosa/magenta para contraste en temas claros/rosa

                System.Windows.Media.Brush colorTextoEjes = TemaManager.EsModoOscuro
                    ? System.Windows.Media.Brushes.White
                    : System.Windows.Media.Brushes.DarkSlateGray;

                var lineaAnimo = new LiveCharts.Wpf.LineSeries
                {
                    Title = "Nivel de Ánimo",
                    Values = valoresPuntuacion,
                    PointGeometrySize = 12,
                    LineSmoothness = 0.6,
                    DataLabels = true,
                    LabelPoint = point => point.Y > 0 ? $"+{point.Y}" : $"{point.Y}",
                    Stroke = colorLinea,
                    Fill = System.Windows.Media.Brushes.Transparent
                };

                chart.Series = new SeriesCollection { lineaAnimo };

                chart.AxisX.Clear();
                chart.AxisX.Add(new WpfAxis
                {
                    Title = "Fecha del Registro",
                    Foreground = colorTextoEjes,
                    Labels = fechasEtiquetas.ToArray(),
                    Separator = new WpfSeparator { Step = 1, IsEnabled = false }
                });

                chart.AxisY.Clear();
                chart.AxisY.Add(new WpfAxis
                {
                    Title = "Estado Emocional (-1 Negativo | +1 Positivo)",
                    Foreground = colorTextoEjes,
                    MinValue = -1.0,
                    MaxValue = 1.0,
                    Separator = new WpfSeparator { Step = 0.5, Stroke = System.Windows.Media.Brushes.LightGray }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos de la gráfica: {ex.Message}");
            }
        }

        private async void CargarAnalisisPsicologia()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://127.0.0.1:5000/api/diario/reporte_psicologica/{idUsuario}");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("puntos_ciegos", out var pcElem))
                    {
                        lblPuntosCiegos.Text = $"PUNTOS CIEGOS DETECTADOS:\n{pcElem.GetString()}";
                    }
                    if (root.TryGetProperty("recomendacion", out var recElem))
                    {
                        lblRecomendacion.Text = recElem.GetString();
                    }
                }
                else
                {
                    lblPuntosCiegos.Text = "No se pudo obtener el reporte clínico del servidor.";
                    lblRecomendacion.Text = "Verifica que la API en Python esté iniciada.";
                }
            }
            catch (Exception ex)
            {
                lblPuntosCiegos.Text = $"Error al procesar el reporte: {ex.Message}";
                lblRecomendacion.Text = "";
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Imagen PNG (*.png)|*.png|Archivo JPG (*.jpg)|*.jpg";
                    saveFileDialog.Title = "Guardar Reporte para la Psicóloga";
                    saveFileDialog.FileName = $"Reporte_Psicologico_{idUsuario}_{DateTime.Now:yyyyMMdd}.png";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                        this.DrawToBitmap(bitmap, new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height));

                        bitmap.Save(saveFileDialog.FileName, ImageFormat.Png);

                        MessageBox.Show("¡Reporte completo guardado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el reporte: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}