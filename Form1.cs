using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class Login : Form
    {
        string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        public static int ClienteLogadoId { get; set; } // Agora armazena o ID do Cliente logado
        public static int ClienteLogadoPerfilId { get; set; } // Para armazenar o PerfilId do Cliente

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
            string nomeCliente = textBoxUsername.Text.Trim(); // Assumindo que o username é o Nome do Cliente
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
                    // Query para verificar Nome e Senha na tabela Cliente
                    "SELECT Id, PerfilId FROM Cliente WHERE Nome = @nomeCliente AND Senha = @senha", conn))
                {
                    cmd.Parameters.AddWithValue("@nomeCliente", nomeCliente);
                    cmd.Parameters.AddWithValue("@senha", password); // Lembre-se de usar hash em produção!

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read()) // Se um cliente é encontrado
                    {
                        ClienteLogadoId = Convert.ToInt32(reader["Id"]);
                        ClienteLogadoPerfilId = Convert.ToInt32(reader["PerfilId"]); // Assumindo que Cliente tem PerfilId

                        reader.Close();

                        destinoForms form3 = new destinoForms();
                        form3.Show();
                        this.Hide();
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
                if (ex.Message.Contains("Invalid object name 'Cliente'"))
                {
                    MessageBox.Show("Erro crítico: A tabela 'Cliente' não foi encontrada no banco de dados. Verifique a configuração do banco.", "Erro de Banco de Dados",
                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Erro ao conectar ao banco: " + ex.Message, "Erro",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Métodos não utilizados do seu ficheiro original (Login_Load, etc.) podem ser mantidos ou removidos
        private void Login_Load(object sender, EventArgs e) { }
        private void guna2TextBox1_TextChanged(object sender, EventArgs e) { }
        private void textBoxPassword_TextChanged(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
    }
}