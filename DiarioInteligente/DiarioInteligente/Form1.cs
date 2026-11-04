using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace DiarioInteligente
{
    public partial class Form1 : Form
    {
        // Cliente HTTP para comunicarse con el servidor Backend en Python
        private static readonly HttpClient client = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 1. Aplica el tema activo global (o el predeterminado)
            TemaManager.AplicarTema(this);
            txtPassword.UseSystemPasswordChar = true;

            // 2. BLOQUEAR CAMBIO DE TAMAÑO Y MAXIMIZADO
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // Evita estirar bordes
            this.MaximizeBox = false;                           // Desactiva el botón de maximizar
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Evento vacío de etiqueta
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string correo = txtCorreo.Text.Trim();
            string contraseña = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(correo))
            {
                MessageBox.Show("Por favor ingresa tu correo o matrícula.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(contraseña))
            {
                MessageBox.Show("Por favor ingresa tu contraseña.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 1. Crear el objeto JSON para enviar a la API
                var datosLogin = new { correo = correo, password = contraseña };
                string jsonBody = JsonSerializer.Serialize(datosLogin);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // 2. Hacer la petición POST a la API de Flask
                HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/api/login", content);

                if (response.IsSuccessStatusCode)
                {
                    // Obtener la respuesta en formato string
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonResponse);
                    var root = doc.RootElement;

                    string idUsuario = root.GetProperty("id").GetString() ?? "";
                    string nombre = root.GetProperty("nombre").GetString() ?? "";
                    string rol = root.GetProperty("rol").GetString() ?? "";

                    // --- CARGAR PREFERENCIA DE TEMA DEL USUARIO DETECTADO ---
                    TemaManager.CargarTemaDeUsuario(idUsuario);

                    MessageBox.Show($"¡Bienvenido(a) {nombre}!\nRol detectado: {rol}", "Acceso Correcto", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // --- REDIRECCIÓN SEGÚN EL ROL DEL USUARIO ---
                    if (rol.ToLower() == "psicologo" || rol.ToLower() == "psicologa" || rol.ToLower() == "especialista")
                    {
                        // Redirige al Panel de la Psicóloga
                        PanelPsicologaForm panelPsicologa = new PanelPsicologaForm();
                        panelPsicologa.Show();
                    }
                    else
                    {
                        // Redirige al Menú del Estudiante
                        HomeFrom home = new HomeFrom(idUsuario, nombre, rol);
                        home.Show();
                    }

                    this.Hide(); // Oculta la pantalla de Login
                }
                else
                {
                    MessageBox.Show("Usuario no encontrado o contraseña incorrecta.", "Error de Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Asegúrate de que la API en Python esté ejecutándose.\n\nDetalle: {ex.Message}", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtCorreo_TextChanged(object sender, EventArgs e)
        {
        }

        // --- MÉTODOS DEL OJITO (MOSTRAR / OCULTAR CONTRASEÑA) ---
        private void btnVerPassword_Click(object sender, EventArgs e)
        {
            // Si la contraseña está oculta, la muestra; de lo contrario la oculta
            if (txtPassword.UseSystemPasswordChar)
            {
                txtPassword.UseSystemPasswordChar = false;
                btnVerPassword.Text = "🙈"; // Icono de ocultar
            }
            else
            {
                txtPassword.UseSystemPasswordChar = true;
                btnVerPassword.Text = "👁️"; // Icono de ver
            }
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void lblRegistrarse_Click(object sender, EventArgs e)
        {
            
            AltaEstudianteForm formRegistro = new AltaEstudianteForm();
            formRegistro.ShowDialog();
        }
    }
}