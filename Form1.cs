using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class Login : Form
    {
        string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        public static int ClienteLogadoId { get; set; }
        public static int ClienteLogadoPerfilId { get; set; }

        public Login()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            CriarContaForm form2 = new CriarContaForm();
            form2.Show();
            this.Hide();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string nomeCliente = textBoxUsername.Text.Trim();
            string password = textBoxPassword.Text;

            if (string.IsNullOrEmpty(nomeCliente) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Preencha o nome do cliente e a senha.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(connString))
                using (var cmd = new SqlCommand(
                    "SELECT Id, PerfilId FROM Cliente WHERE Nome = @nomeCliente AND Senha = @senha", conn))
                {
                    cmd.Parameters.AddWithValue("@nomeCliente", nomeCliente);
                    cmd.Parameters.AddWithValue("@senha", password);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        ClienteLogadoId = Convert.ToInt32(reader["Id"]);
                        ClienteLogadoPerfilId = Convert.ToInt32(reader["PerfilId"]);

                        reader.Close();
                        this.Hide();

                        if (ClienteLogadoPerfilId == 2) // O ID 2 é para 'Admin'
                        {
                            MessageBox.Show("Bem-vindo, Admin!", "Login de Administrador", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            adminForms painelAdmin = new adminForms();
                            painelAdmin.Show();
                        }
                        else
                        {
                            menuForms formMenu = new menuForms();
                            formMenu.Show();
                        }
                    }
                    else
                    {
                        reader.Close();
                        MessageBox.Show("Nome do cliente ou senha incorretos.", "Erro de Login",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar ao banco: " + ex.Message, "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- Manter os outros métodos que você possa ter ---
        private void Login_Load(object sender, EventArgs e) { }
        private void guna2TextBox1_TextChanged(object sender, EventArgs e) { }
        private void textBoxPassword_TextChanged(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
    }
}