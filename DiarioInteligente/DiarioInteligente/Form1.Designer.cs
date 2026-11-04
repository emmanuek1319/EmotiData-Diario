namespace DiarioInteligente
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblCorreo = new Label();
            lblPassword = new Label();
            txtCorreo = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            btnVerPassword = new Button();
            label1 = new Label();
            lblRegistrarse = new Label();
            SuspendLayout();
            // 
            // lblCorreo
            // 
            lblCorreo.AutoSize = true;
            lblCorreo.BackColor = Color.Transparent;
            lblCorreo.Font = new Font("Candara", 14F);
            lblCorreo.ForeColor = SystemColors.MenuText;
            lblCorreo.Location = new Point(790, 152);
            lblCorreo.Name = "lblCorreo";
            lblCorreo.Size = new Size(234, 35);
            lblCorreo.TabIndex = 0;
            lblCorreo.Text = "Correo / Matrícula:";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.BackColor = Color.Transparent;
            lblPassword.Font = new Font("Candara", 14F);
            lblPassword.ForeColor = SystemColors.MenuText;
            lblPassword.Location = new Point(835, 315);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(157, 35);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Contraseña:";
            // 
            // txtCorreo
            // 
            txtCorreo.ForeColor = SystemColors.MenuText;
            txtCorreo.Location = new Point(759, 208);
            txtCorreo.Margin = new Padding(3, 4, 3, 4);
            txtCorreo.Name = "txtCorreo";
            txtCorreo.Size = new Size(280, 31);
            txtCorreo.TabIndex = 1;
            txtCorreo.TextChanged += txtCorreo_TextChanged;
            // 
            // txtPassword
            // 
            txtPassword.ForeColor = SystemColors.MenuText;
            txtPassword.Location = new Point(759, 387);
            txtPassword.Margin = new Padding(3, 4, 3, 4);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(280, 31);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogin.ForeColor = SystemColors.MenuText;
            btnLogin.Location = new Point(835, 497);
            btnLogin.Margin = new Padding(3, 4, 3, 4);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(141, 49);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "Iniciar";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // btnVerPassword
            // 
            btnVerPassword.FlatStyle = FlatStyle.Flat;
            btnVerPassword.Font = new Font("Segoe UI", 9F);
            btnVerPassword.ForeColor = SystemColors.MenuText;
            btnVerPassword.Location = new Point(1080, 385);
            btnVerPassword.Name = "btnVerPassword";
            btnVerPassword.Size = new Size(40, 33);
            btnVerPassword.TabIndex = 4;
            btnVerPassword.Text = "👁️";
            btnVerPassword.UseVisualStyleBackColor = true;
            btnVerPassword.Click += btnVerPassword_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Sitka Small", 14F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.MenuText;
            label1.Location = new Point(626, 70);
            label1.Name = "label1";
            label1.Size = new Size(540, 40);
            label1.TabIndex = 6;
            label1.Text = "Bienvenido a tu Diario Inteligente  ";
            label1.TextAlign = ContentAlignment.TopCenter;
            label1.Click += label1_Click_1;
            // 
            // lblRegistrarse
            // 
            lblRegistrarse.AutoSize = true;
            lblRegistrarse.BackColor = Color.Transparent;
            lblRegistrarse.Location = new Point(776, 578);
            lblRegistrarse.Name = "lblRegistrarse";
            lblRegistrarse.Size = new Size(282, 25);
            lblRegistrarse.TabIndex = 7;
            lblRegistrarse.Text = "¿No tienes cuenta? Regístrate aquí";
            lblRegistrarse.Click += lblRegistrarse_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BackgroundImage = Properties.Resources.Gemini_Generated_Image_pql05qpql05qpql0__1_;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1200, 667);
            Controls.Add(lblRegistrarse);
            Controls.Add(label1);
            Controls.Add(btnVerPassword);
            Controls.Add(btnLogin);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtCorreo);
            Controls.Add(lblCorreo);
            DoubleBuffered = true;
            ForeColor = SystemColors.Desktop;
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Inicio de Sesión";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblCorreo;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtCorreo;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnVerPassword;
        private Label label1;
        private Label lblRegistrarse;
    }
}