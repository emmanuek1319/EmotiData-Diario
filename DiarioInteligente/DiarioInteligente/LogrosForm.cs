using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace DiarioInteligente
{
    public partial class LogrosForm : Form
    {
        private string idUsuario;
        private HomeFrom menuPrincipal;
        private static readonly HttpClient client = new HttpClient();

        public LogrosForm(string id, HomeFrom home)
        {
            InitializeComponent();
            this.idUsuario = id;
            this.menuPrincipal = home;
        }

        private void LogrosForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;

            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            // 1. CARGAR Y APLICAR EL TEMA DEL USUARIO
            TemaManager.CargarTemaDeUsuario(this.idUsuario);
            TemaManager.AplicarTema(this);

            // 2. Cargar datos de los logros
            CargarLogros();
        }

        private async void CargarLogros()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://127.0.0.1:5000/api/usuario/logros/{idUsuario}");

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonResponse);
                    var root = doc.RootElement;

                    // 1. Datos del Header desde app.py
                    int racha = root.TryGetProperty("racha_actual", out var r) ? r.GetInt32() : 0;
                    int desbloqueados = root.TryGetProperty("desbloqueados_conteo", out var d) ? d.GetInt32() : 0;
                    int totalLogros = root.TryGetProperty("total_logros", out var t) ? t.GetInt32() : 0;

                    string textoRachaMedallas = $"🔥 Racha: {racha} días  |  🏅 Medallas: {desbloqueados}/{totalLogros}";
                    string textoProximo = "🎯 Próximo logro: Cuidando tu constancia";

                    if (root.TryGetProperty("proximo_logro", out var proximoObj) && proximoObj.ValueKind != JsonValueKind.Null)
                    {
                        textoProximo = proximoObj.TryGetProperty("leyenda", out var ley) ? ley.GetString() ?? textoProximo : textoProximo;
                    }

                    // Forzamos actualización recursiva en toda la ventana
                    ActualizarTextoEtiquetas(this, textoRachaMedallas, textoProximo);

                    // 2. Render de Tarjetas en el Contenedor
                    Control contenedor = ObtenerContenedorPrincipal();

                    if (contenedor != null)
                    {
                        contenedor.Controls.Clear();

                        if (root.TryGetProperty("logros", out var arrayLogros) && arrayLogros.ValueKind == JsonValueKind.Array)
                        {
                            int topPosition = 10;
                            int anchoTarjeta = contenedor.Width - 35;

                            foreach (var logroElem in arrayLogros.EnumerateArray())
                            {
                                string titulo = logroElem.TryGetProperty("titulo", out var tit) ? tit.GetString() ?? "" : "Logro";
                                string descripcion = logroElem.TryGetProperty("descripcion", out var desc) ? desc.GetString() ?? "" : "";
                                bool desbloqueado = logroElem.TryGetProperty("desbloqueado", out var des) && des.GetBoolean();
                                string icono = logroElem.TryGetProperty("icono", out var ico) ? ico.GetString() : (desbloqueado ? "🏆" : "🔒");

                                Panel tarjeta = CrearTarjetaLogro(titulo, descripcion, icono, desbloqueado, anchoTarjeta, topPosition);
                                contenedor.Controls.Add(tarjeta);

                                topPosition += 75;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Manejo de excepción silencioso
            }
        }

        private Panel CrearTarjetaLogro(string titulo, string descripcion, string icono, bool desbloqueado, int ancho, int topPos)
        {
            Panel card = new Panel();
            card.Size = new Size(ancho > 150 ? ancho : 400, 65);
            card.Location = new Point(10, topPos);
            card.BorderStyle = BorderStyle.FixedSingle;

            // Ajuste sutil del fondo de tarjeta según el estado del tema
            if (TemaManager.EsModoOscuro)
            {
                card.BackColor = desbloqueado ? Color.FromArgb(30, 60, 45) : Color.FromArgb(40, 42, 50);
            }
            else
            {
                card.BackColor = desbloqueado ? Color.FromArgb(230, 245, 230) : Color.FromArgb(245, 243, 240);
            }

            // Icono
            Label lblIcono = new Label();
            lblIcono.Text = icono;
            lblIcono.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblIcono.Location = new Point(10, 12);
            lblIcono.Size = new Size(40, 40);
            lblIcono.BackColor = Color.Transparent;
            card.Controls.Add(lblIcono);

            // Título
            Label lblTitulo = new Label();
            lblTitulo.Text = titulo;
            lblTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTitulo.ForeColor = desbloqueado ? (TemaManager.EsModoOscuro ? Color.LightGreen : Color.DarkGreen) : Color.Gray;
            lblTitulo.Location = new Point(65, 8);
            lblTitulo.AutoSize = true;
            lblTitulo.BackColor = Color.Transparent;
            card.Controls.Add(lblTitulo);

            // Descripción
            Label lblDesc = new Label();
            lblDesc.Text = descripcion;
            lblDesc.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            lblDesc.ForeColor = TemaManager.EsModoOscuro ? Color.Gainsboro : Color.DimGray;
            lblDesc.Location = new Point(65, 30);
            lblDesc.Size = new Size(ancho - 80, 30);
            lblDesc.BackColor = Color.Transparent;
            card.Controls.Add(lblDesc);

            return card;
        }

        // Búsqueda profunda para encontrar las etiquetas sin importar en qué Panel o GroupBox estén
        private void ActualizarTextoEtiquetas(Control padre, string textoRacha, string textoProximo)
        {
            foreach (Control c in padre.Controls)
            {
                if (c is Label lbl)
                {
                    if (lbl.Text.Contains("Racha") || lbl.Name.Contains("Racha") || lbl.Name.Contains("Medalla"))
                    {
                        lbl.Text = textoRacha;
                    }
                    else if (lbl.Text.Contains("Próximo") || lbl.Name.Contains("Proximo") || lbl.Name.Contains("Meta"))
                    {
                        lbl.Text = textoProximo;
                    }
                }

                if (c.HasChildren)
                {
                    ActualizarTextoEtiquetas(c, textoRacha, textoProximo);
                }
            }
        }

        private Control ObtenerContenedorPrincipal()
        {
            foreach (Control c in this.Controls)
            {
                if (c is FlowLayoutPanel || (c is Panel && c.Height > 200))
                {
                    return c;
                }
            }
            return this;
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