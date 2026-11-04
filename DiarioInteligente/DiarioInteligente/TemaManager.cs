using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DiarioInteligente
{
    public enum TipoTema
    {
        OceanoOscuro,
        CafeCrema,
        VerdeSalvia,
        AtardecerCarmesi,
        LavandaMistica,
        UrbanoNaranja,
        PurpuraNoche,
        Emotidata
    }

    public static class TemaManager
    {
        private static string idUsuarioActual = "default";

        // Mantenemos una propiedad pública con getter/setter privado
        public static TipoTema TemaActual { get; private set; } = TipoTema.CafeCrema;

        public static bool EsModoOscuro => TemaActual == TipoTema.OceanoOscuro ||
                                           TemaActual == TipoTema.AtardecerCarmesi ||
                                           TemaActual == TipoTema.PurpuraNoche ||
                                           TemaActual == TipoTema.UrbanoNaranja;

        // --- PALETAS ---
        // 1. Océano Oscuro
        public static readonly Color Oscuro_Fondo = ColorTranslator.FromHtml("#0A1931");
        public static readonly Color Oscuro_Panel = ColorTranslator.FromHtml("#1A3D63");
        public static readonly Color Oscuro_Boton = ColorTranslator.FromHtml("#4A7FA7");
        public static readonly Color Oscuro_Acento = ColorTranslator.FromHtml("#B3CFE5");
        public static readonly Color Oscuro_Texto = ColorTranslator.FromHtml("#F6FAFD");

        // 2. Café & Crema
        public static readonly Color Cafe_Fondo = ColorTranslator.FromHtml("#E1D4C2");
        public static readonly Color Cafe_Panel = ColorTranslator.FromHtml("#BEB5A9");
        public static readonly Color Cafe_BotonVolver = ColorTranslator.FromHtml("#A78D78");
        public static readonly Color Cafe_Boton = ColorTranslator.FromHtml("#6E473B");
        public static readonly Color Cafe_Texto = ColorTranslator.FromHtml("#291C0E");

        // 3. Verde Salvia
        public static readonly Color Salvia_Fondo = ColorTranslator.FromHtml("#E7F5DC");
        public static readonly Color Salvia_Panel = ColorTranslator.FromHtml("#CFE1B9");
        public static readonly Color Salvia_BotonVolver = ColorTranslator.FromHtml("#B6C99B");
        public static readonly Color Salvia_Boton = ColorTranslator.FromHtml("#728156");
        public static readonly Color Salvia_Texto = ColorTranslator.FromHtml("#2E3A1E");

        // 4. Atardecer Carmesí
        public static readonly Color Carmesi_Fondo = ColorTranslator.FromHtml("#181A2F");
        public static readonly Color Carmesi_Panel = ColorTranslator.FromHtml("#242E49");
        public static readonly Color Carmesi_BotonVolver = ColorTranslator.FromHtml("#37415C");
        public static readonly Color Carmesi_Boton = ColorTranslator.FromHtml("#B4182D");
        public static readonly Color Carmesi_Texto = ColorTranslator.FromHtml("#F6FAFD");

        // 5. Lavanda Mística
        public static readonly Color Lavanda_Fondo = ColorTranslator.FromHtml("#F3CCDE");
        public static readonly Color Lavanda_Panel = ColorTranslator.FromHtml("#D6A8C4");
        public static readonly Color Lavanda_BotonVolver = ColorTranslator.FromHtml("#BA88AE");
        public static readonly Color Lavanda_Boton = ColorTranslator.FromHtml("#5B3765");
        public static readonly Color Lavanda_Texto = ColorTranslator.FromHtml("#2D1633");

        // 6. Urbano Naranja (Urban Palette - Noguchi)
        public static readonly Color Urbano_Fondo = ColorTranslator.FromHtml("#3A3F43");
        public static readonly Color Urbano_Panel = ColorTranslator.FromHtml("#666C7B");
        public static readonly Color Urbano_BotonVolver = ColorTranslator.FromHtml("#666C7B");
        public static readonly Color Urbano_Boton = ColorTranslator.FromHtml("#DC5F00");
        public static readonly Color Urbano_Texto = ColorTranslator.FromHtml("#EEEEEE");

        // 7. Púrpura Noche (Purple Palette - Noguchi)
        public static readonly Color Purpura_Fondo = ColorTranslator.FromHtml("#1E202C");
        public static readonly Color Purpura_Panel = ColorTranslator.FromHtml("#31323E");
        public static readonly Color Purpura_BotonVolver = ColorTranslator.FromHtml("#31323E");
        public static readonly Color Purpura_Boton = ColorTranslator.FromHtml("#60519B");
        public static readonly Color Purpura_Texto = ColorTranslator.FromHtml("#BFC0D1");

        // 8. Emotidata (Identidad Institucional / Corporativa)
        public static readonly Color Emotidata_Fondo = ColorTranslator.FromHtml("#F8F9FA");
        public static readonly Color Emotidata_Panel = ColorTranslator.FromHtml("#E2E8F0");
        public static readonly Color Emotidata_BotonVolver = ColorTranslator.FromHtml("#4A7BB0");
        public static readonly Color Emotidata_Boton = ColorTranslator.FromHtml("#1A365D");
        public static readonly Color Emotidata_Texto = ColorTranslator.FromHtml("#1A365D");

        // --- PERSISTENCIA CORREGIDA POR USUARIO ---

        public static void CargarTemaDeUsuario(string idUsuario)
        {
            if (!string.IsNullOrWhiteSpace(idUsuario))
            {
                idUsuarioActual = idUsuario.Trim();
            }

            string archivo = ObtenerRutaArchivoConfig();

            try
            {
                if (File.Exists(archivo))
                {
                    string contenido = File.ReadAllText(archivo).Trim();
                    if (Enum.TryParse(contenido, out TipoTema temaGuardado))
                    {
                        TemaActual = temaGuardado; // Fuerza la actualización en memoria
                        return;
                    }
                }
            }
            catch { }

            TemaActual = TipoTema.CafeCrema; // Valor predeterminado si es un usuario nuevo
        }

        private static string ObtenerRutaArchivoConfig()
        {
            // Se guarda en %AppData%\DiarioInteligente para evitar bloqueos de permisos de lectura/escritura
            string carpetaAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiarioInteligente");

            if (!Directory.Exists(carpetaAppData))
            {
                Directory.CreateDirectory(carpetaAppData);
            }

            return Path.Combine(carpetaAppData, $"tema_user_{idUsuarioActual}.cfg");
        }

        private static void GuardarTemaActual()
        {
            try
            {
                string archivo = ObtenerRutaArchivoConfig();
                File.WriteAllText(archivo, TemaActual.ToString());
            }
            catch { }
        }

        public static void EstablecerTema(TipoTema nuevoTema, Form formulario = null)
        {
            TemaActual = nuevoTema;
            GuardarTemaActual();

            if (formulario != null)
            {
                AplicarTema(formulario);
            }
        }

        public static TipoTema SiguienteTema(Form formulario = null)
        {
            switch (TemaActual)
            {
                case TipoTema.OceanoOscuro: TemaActual = TipoTema.CafeCrema; break;
                case TipoTema.CafeCrema: TemaActual = TipoTema.VerdeSalvia; break;
                case TipoTema.VerdeSalvia: TemaActual = TipoTema.AtardecerCarmesi; break;
                case TipoTema.AtardecerCarmesi: TemaActual = TipoTema.LavandaMistica; break;
                case TipoTema.LavandaMistica: TemaActual = TipoTema.UrbanoNaranja; break;
                case TipoTema.UrbanoNaranja: TemaActual = TipoTema.PurpuraNoche; break;
                case TipoTema.PurpuraNoche: TemaActual = TipoTema.Emotidata; break;
                case TipoTema.Emotidata:
                default: TemaActual = TipoTema.OceanoOscuro; break;
            }

            GuardarTemaActual();

            if (formulario != null)
            {
                AplicarTema(formulario);
            }

            return TemaActual;
        }

        // --- MÉTODOS DE COMPATIBILIDAD ---
        public static void AplicarTemaOscuro(Form formulario)
        {
            EstablecerTema(TipoTema.OceanoOscuro, formulario);
        }

        public static bool AlternarTema(Form formulario = null)
        {
            SiguienteTema(formulario);
            return EsModoOscuro;
        }

        // --- APLICACIÓN GENERAL DE ESTILOS ---
        public static void AplicarTema(Form formulario)
        {
            if (formulario == null) return;

            Color fondo, panel, texto, botonPrimary, botonSec, inputBg;

            switch (TemaActual)
            {
                case TipoTema.OceanoOscuro:
                    fondo = Oscuro_Fondo; panel = Oscuro_Panel; texto = Oscuro_Texto;
                    botonPrimary = Oscuro_Boton; botonSec = Oscuro_Panel; inputBg = Oscuro_Fondo;
                    break;

                case TipoTema.VerdeSalvia:
                    fondo = Salvia_Fondo; panel = Salvia_Panel; texto = Salvia_Texto;
                    botonPrimary = Salvia_Boton; botonSec = Salvia_BotonVolver; inputBg = Color.FromArgb(248, 252, 245);
                    break;

                case TipoTema.AtardecerCarmesi:
                    fondo = Carmesi_Fondo; panel = Carmesi_Panel; texto = Carmesi_Texto;
                    botonPrimary = Carmesi_Boton; botonSec = Carmesi_BotonVolver; inputBg = Carmesi_Panel;
                    break;

                case TipoTema.LavandaMistica:
                    fondo = Lavanda_Fondo; panel = Lavanda_Panel; texto = Lavanda_Texto;
                    botonPrimary = Lavanda_Boton; botonSec = Lavanda_BotonVolver; inputBg = Color.FromArgb(252, 242, 247);
                    break;

                case TipoTema.UrbanoNaranja:
                    fondo = Urbano_Fondo; panel = Urbano_Panel; texto = Urbano_Texto;
                    botonPrimary = Urbano_Boton; botonSec = Urbano_BotonVolver; inputBg = Urbano_Panel;
                    break;

                case TipoTema.PurpuraNoche:
                    fondo = Purpura_Fondo; panel = Purpura_Panel; texto = Purpura_Texto;
                    botonPrimary = Purpura_Boton; botonSec = Purpura_BotonVolver; inputBg = Purpura_Panel;
                    break;

                case TipoTema.Emotidata:
                    fondo = Emotidata_Fondo; panel = Emotidata_Panel; texto = Emotidata_Texto;
                    botonPrimary = Emotidata_Boton; botonSec = Emotidata_BotonVolver; inputBg = Color.White;
                    break;

                case TipoTema.CafeCrema:
                default:
                    fondo = Cafe_Fondo; panel = Cafe_Panel; texto = Cafe_Texto;
                    botonPrimary = Cafe_Boton; botonSec = Cafe_BotonVolver; inputBg = Color.FromArgb(248, 244, 238);
                    break;
            }

            formulario.BackColor = fondo;
            formulario.ForeColor = texto;

            AplicarEstiloRecursivo(formulario, fondo, panel, texto, botonPrimary, botonSec, inputBg);

            formulario.Invalidate();
            formulario.Refresh();
        }

        private static void AplicarEstiloRecursivo(Control padre, Color fondo, Color panel, Color texto, Color botonPrimary, Color botonSec, Color inputBg)
        {
            foreach (Control c in padre.Controls)
            {
                if (c is Panel || c is GroupBox)
                {
                    c.BackColor = (c.Dock == DockStyle.Fill || c.Height > padre.Height * 0.8) ? fondo : panel;
                    c.ForeColor = texto;
                }
                else if (c is Button btn)
                {
                    btn.UseVisualStyleBackColor = false;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

                    if (btn.Name.Contains("Volver") || btn.Name.Contains("Cancelar") || btn.Text.Contains("Volver"))
                    {
                        btn.BackColor = botonSec;
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.BorderColor = texto;
                    }
                    else
                    {
                        btn.BackColor = botonPrimary;
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.BorderColor = texto;
                    }
                }
                else if (c is TextBox txt)
                {
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    txt.BackColor = inputBg;
                    txt.ForeColor = texto;
                }
                else if (c is Label lbl)
                {
                    lbl.ForeColor = texto;
                    lbl.BackColor = Color.Transparent;
                }
                else if (c is DataGridView dgv)
                {
                    EstilarTabla(dgv);
                }

                if (c.HasChildren)
                {
                    AplicarEstiloRecursivo(c, fondo, panel, texto, botonPrimary, botonSec, inputBg);
                }
            }
        }

        public static void EstilarTabla(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            Color bg, panel, text, btn;

            switch (TemaActual)
            {
                case TipoTema.OceanoOscuro: bg = Oscuro_Fondo; panel = Oscuro_Panel; text = Oscuro_Texto; btn = Oscuro_Boton; break;
                case TipoTema.AtardecerCarmesi: bg = Carmesi_Fondo; panel = Carmesi_Panel; text = Carmesi_Texto; btn = Carmesi_Boton; break;
                case TipoTema.VerdeSalvia: bg = Salvia_Fondo; panel = Color.FromArgb(248, 252, 245); text = Salvia_Texto; btn = Salvia_Boton; break;
                case TipoTema.LavandaMistica: bg = Lavanda_Fondo; panel = Color.FromArgb(252, 242, 247); text = Lavanda_Texto; btn = Lavanda_Boton; break;
                case TipoTema.UrbanoNaranja: bg = Urbano_Fondo; panel = Urbano_Panel; text = Urbano_Texto; btn = Urbano_Boton; break;
                case TipoTema.PurpuraNoche: bg = Purpura_Fondo; panel = Purpura_Panel; text = Purpura_Texto; btn = Purpura_Boton; break;
                case TipoTema.Emotidata: bg = Emotidata_Fondo; panel = Color.White; text = Emotidata_Texto; btn = Emotidata_Boton; break;
                case TipoTema.CafeCrema:
                default: bg = Cafe_Fondo; panel = Color.FromArgb(248, 244, 238); text = Cafe_Texto; btn = Cafe_Boton; break;
            }

            dgv.BackgroundColor = bg;
            dgv.DefaultCellStyle.BackColor = panel;
            dgv.DefaultCellStyle.ForeColor = text;
            dgv.DefaultCellStyle.SelectionBackColor = btn;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = bg;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = text;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgv.GridColor = btn;
        }
    }
}