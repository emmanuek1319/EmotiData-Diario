using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace DiarioInteligente
{
    public partial class ZonaRelaxForm : Form
    {
        private static readonly HttpClient client = new HttpClient();
        private List<FraseItem> listaFrases = new List<FraseItem>();
        private int indiceFraseActual = 0;

        // Variables de respiración guiada (4 - 7 - 8)
        private enum FaseRespiracion { Inhalar, Mantener, Exhalar }
        private FaseRespiracion faseActual = FaseRespiracion.Inhalar;
        private int contadorSegundos = 4;
        private bool respiracionActiva = false;

        public class FraseItem
        {
            public string texto { get; set; }
            public string autor { get; set; }
        }

        public ZonaRelaxForm()
        {
            InitializeComponent();
        }

        private void ZonaRelaxForm_Load(object sender, EventArgs e)
        {
            // 1. Centrado y Bloqueo de Tamaño
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;

            // 2. APLICAR TEMA ACTIVO DE LA APLICACIÓN
            TemaManager.AplicarTema(this);

            // 3. Cargar frases relajantes y arrancar temporizador de frases
            CargarFrasesDeMongo();
            timerFrases.Start();
        }

        private async void CargarFrasesDeMongo()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:5000/api/relax/frases");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("frases", out var arrayFrases))
                    {
                        listaFrases.Clear();
                        foreach (var f in arrayFrases.EnumerateArray())
                        {
                            listaFrases.Add(new FraseItem
                            {
                                texto = f.GetProperty("texto").GetString(),
                                autor = f.TryGetProperty("autor", out var pAut) ? pAut.GetString() : "Anónimo"
                            });
                        }

                        if (listaFrases.Count > 0)
                        {
                            MostrarFraseActual();
                        }
                    }
                }
            }
            catch
            {
                lblFraseTexto.Text = "\"Un paso a la vez es suficiente para avanzar hacia la calma.\"";
                lblFraseAutor.Text = "- Diario Inteligente";
            }
        }

        private void MostrarFraseActual()
        {
            if (listaFrases.Count == 0) return;

            var f = listaFrases[indiceFraseActual];
            lblFraseTexto.Text = $"\"{f.texto}\"";
            lblFraseAutor.Text = $"- {f.autor}";
        }

        private void timerFrases_Tick(object sender, EventArgs e)
        {
            if (listaFrases.Count > 0)
            {
                indiceFraseActual = (indiceFraseActual + 1) % listaFrases.Count;
                MostrarFraseActual();
            }
        }

        private void btnSiguienteFrase_Click(object sender, EventArgs e)
        {
            if (listaFrases.Count > 0)
            {
                indiceFraseActual = (indiceFraseActual + 1) % listaFrases.Count;
                MostrarFraseActual();
            }
        }

        // --- LÓGICA DE RESPIRACIÓN GUIADA (4-7-8) ---
        private void btnIniciarRespiracion_Click(object sender, EventArgs e)
        {
            if (!respiracionActiva)
            {
                respiracionActiva = true;
                btnIniciarRespiracion.Text = "⏹️ Detener";
                btnIniciarRespiracion.BackColor = Color.IndianRed;
                btnIniciarRespiracion.ForeColor = Color.White;

                faseActual = FaseRespiracion.Inhalar;
                contadorSegundos = 4;
                ActualizarInterfazRespiracion();
                timerRespiracion.Start();
            }
            else
            {
                respiracionActiva = false;
                timerRespiracion.Stop();
                btnIniciarRespiracion.Text = "▶️ Iniciar Guiado";

                // Re-aplica estilos para restaurar color primario del tema en el botón
                TemaManager.AplicarTema(this);

                lblEstadoRespiracion.Text = "Ejercicio pausado. Presiona iniciar cuando desees continuar.";
                lblCirculoRespiracion.Size = new Size(90, 90);
                lblCirculoRespiracion.Location = new Point(340, 35);
            }
        }

        private void timerRespiracion_Tick(object sender, EventArgs e)
        {
            contadorSegundos--;

            if (contadorSegundos <= 0)
            {
                switch (faseActual)
                {
                    case FaseRespiracion.Inhalar:
                        faseActual = FaseRespiracion.Mantener;
                        contadorSegundos = 7;
                        break;
                    case FaseRespiracion.Mantener:
                        faseActual = FaseRespiracion.Exhalar;
                        contadorSegundos = 8;
                        break;
                    case FaseRespiracion.Exhalar:
                        faseActual = FaseRespiracion.Inhalar;
                        contadorSegundos = 4;
                        break;
                }
            }

            ActualizarInterfazRespiracion();
        }

        private void ActualizarInterfazRespiracion()
        {
            lblCirculoRespiracion.Text = contadorSegundos.ToString();
            lblCirculoRespiracion.ForeColor = Color.White;

            switch (faseActual)
            {
                case FaseRespiracion.Inhalar:
                    lblEstadoRespiracion.Text = "🫁 INHALA profundamente por la nariz...";
                    lblCirculoRespiracion.BackColor = Color.DeepSkyBlue;
                    lblCirculoRespiracion.Size = new Size(110, 110);
                    lblCirculoRespiracion.Location = new Point(330, 25);
                    break;

                case FaseRespiracion.Mantener:
                    lblEstadoRespiracion.Text = "⏸️ MANTÉN el aire en tus pulmones...";
                    lblCirculoRespiracion.BackColor = Color.Goldenrod;
                    lblCirculoRespiracion.Size = new Size(110, 110);
                    lblCirculoRespiracion.Location = new Point(330, 25);
                    break;

                case FaseRespiracion.Exhalar:
                    lblEstadoRespiracion.Text = "😮‍💨 EXHALA despacio por la boca...";
                    lblCirculoRespiracion.BackColor = Color.MediumSeaGreen;
                    lblCirculoRespiracion.Size = new Size(80, 80);
                    lblCirculoRespiracion.Location = new Point(345, 40);
                    break;
            }
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            timerFrases.Stop();
            timerRespiracion.Stop();
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string url = "https://www.gob.mx/conasama/articulos/linea-de-la-vida-800-911-2000";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show(
                    "Puedes marcar gratuitamente al 800 911 2000 o ingresar a:\nhttps://www.gob.mx/conasama/articulos/linea-de-la-vida-800-911-2000",
                    "Línea de la Vida (Atención 24/7)",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
    }
}