using System;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace DiarioInteligente
{
    public partial class HomeFrom : Form
    {
        private string idUsuario;
        private string nombreUsuario;
        private string rolUsuario;
        private static readonly HttpClient client = new HttpClient();

        // Constructor por defecto (para el diseñador visual)
        public HomeFrom()
        {
            InitializeComponent();
            this.idUsuario = "USR_DEMO";
            this.nombreUsuario = "Usuario";
            this.rolUsuario = "estudiante";
        }

        // Constructor principal que recibe los datos desde el Login
        public HomeFrom(string id, string nombre, string rol)
        {
            InitializeComponent();
            this.idUsuario = id;
            this.nombreUsuario = nombre;
            this.rolUsuario = rol;
        }

        private void HomeFrom_Load(object sender, EventArgs e)
        {
            // 1. BLOQUEAR TAMAÑO Y MAXIMIZADO (Mantiene la forma limpia y fija)
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // Evita estirar los bordes
            this.MaximizeBox = false;                           // Desactiva el botón de maximizar

            // 2. CARGAR EL TEMA PERSONALIZADO DEL USUARIO QUE INICIÓ SESIÓN
            TemaManager.CargarTemaDeUsuario(this.idUsuario);
            TemaManager.AplicarTema(this);
            ActualizarTextoBotonTema();

            // 3. Saludo personalizado y carga de datos desde el backend
            lblBienvenida.Text = $"¡Hola, {nombreUsuario}!";
            CargarResumenUsuario();
        }

        // Método público para cargar el resumen de racha, última emoción y próximo logro
        public async void CargarResumenUsuario()
        {
            try
            {
                // 1. Petición al backend para obtener el resumen (racha y última emoción)
                HttpResponseMessage responseResumen = await client.GetAsync($"http://127.0.0.1:5000/api/usuario/resumen/{idUsuario}");

                if (responseResumen.IsSuccessStatusCode)
                {
                    string jsonResponse = await responseResumen.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonResponse);
                    var root = doc.RootElement;

                    int racha = root.GetProperty("racha").GetInt32();
                    string ultimaEmocion = root.GetProperty("ultima_emocion").GetString() ?? "Sin registros";
                    string fechaUltima = root.GetProperty("fecha_ultima").GetString() ?? "--";

                    lblRacha.Text = $"🔥 Racha activa: {racha} días";
                    lblEmocion.Text = $"Emoción: {ultimaEmocion}";
                    lblFechaEmocion.Text = $"Fecha: {fechaUltima}";
                }

                // 2. Petición al backend para obtener el próximo logro y su leyenda
                HttpResponseMessage responseLogros = await client.GetAsync($"http://127.0.0.1:5000/api/usuario/logros/{idUsuario}");

                if (responseLogros.IsSuccessStatusCode)
                {
                    string jsonLogros = await responseLogros.Content.ReadAsStringAsync();
                    using var docLogros = JsonDocument.Parse(jsonLogros);
                    var rootLogros = docLogros.RootElement;

                    if (rootLogros.TryGetProperty("proximo_logro", out var proximoLogroObj))
                    {
                        if (proximoLogroObj.TryGetProperty("leyenda", out var leyendaElem))
                        {
                            string leyenda = leyendaElem.GetString() ?? "";

                            // Asigna la leyenda calculada por Python directamente al Label:
                            lblProximoLogro.Text = leyenda;
                        }
                    }
                }
            }
            catch
            {
                // Muestra valores por defecto si el servidor Python no está encendido
                lblRacha.Text = "🔥 Racha activa: -- días";
                lblEmocion.Text = "Emoción: Sin conexión";
                lblFechaEmocion.Text = "Fecha: --";
            }
        }

        private void btnNuevaEntrada_Click(object sender, EventArgs e)
        {
            // Abrir la Ventana de Registro
            RegistroEmocionalForm formularioRegistro = new RegistroEmocionalForm(idUsuario, this);
            formularioRegistro.Show();
            this.Hide();
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Form1 login = new Form1();
            login.Show();
            this.Close(); // Regresa al Login
        }

        private void btnVerHistorial_Click(object sender, EventArgs e)
        {
            HistorialForm pantallaHistorial = new HistorialForm(idUsuario, this);
            pantallaHistorial.Show();
            this.Hide();
        }

        private void Estadisticasbton_Click(object sender, EventArgs e)
        {
            EstadisticasForm estadisticasVentana = new EstadisticasForm(idUsuario);
            estadisticasVentana.Show();
        }

        private void btnLogros_Click(object sender, EventArgs e)
        {
            LogrosForm pantallaLogros = new LogrosForm(idUsuario, this);
            pantallaLogros.Show();
            this.Hide();
        }

        private void btnZonaRelax_Click(object sender, EventArgs e)
        {
            ZonaRelaxForm zonaRelax = new ZonaRelaxForm();
            zonaRelax.ShowDialog();
        }

        // --- EVENTO Y MÉTODOS DEL SELECTOR DE TEMA ---

        private void btnCambiarTema_Click(object sender, EventArgs e)
        {
            // Cambia al siguiente tema en la secuencia de 7 temas sin alertas
            TemaManager.SiguienteTema(this);
            ActualizarTextoBotonTema();
        }

        private void ActualizarTextoBotonTema()
        {
            // Muestra en el botón cuál tema está activo de forma elegante
            switch (TemaManager.TemaActual)
            {
                case TipoTema.OceanoOscuro:
                    btnCambiarTema.Text = "🎨 Tema: Océano Oscuro";
                    break;
                case TipoTema.CafeCrema:
                    btnCambiarTema.Text = "🎨 Tema: Café & Crema";
                    break;
                case TipoTema.VerdeSalvia:
                    btnCambiarTema.Text = "🎨 Tema: Verde Salvia";
                    break;
                case TipoTema.AtardecerCarmesi:
                    btnCambiarTema.Text = "🎨 Tema: Atardecer Carmesí";
                    break;
                case TipoTema.LavandaMistica:
                    btnCambiarTema.Text = "🎨 Tema: Lavanda Mística";
                    break;
                case TipoTema.UrbanoNaranja:
                    btnCambiarTema.Text = "🎨 Tema: Urbano Naranja";
                    break;
                case TipoTema.PurpuraNoche:
                    btnCambiarTema.Text = "🎨 Tema: Púrpura Noche";
                    break;
            }
        }
    }
}