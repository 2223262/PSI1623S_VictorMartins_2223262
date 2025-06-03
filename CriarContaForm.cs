using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class CriarContaForm : Form
    {
        string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";

        public CriarContaForm()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e) // Botão "Criar"
        {
            string nomeCliente = textBoxUsername.Text.Trim(); // Assumindo que o username é o Nome do Cliente
            string password = textBoxPassword.Text;
            int perfilIdPadrao = 1; // Ex: Perfil "Passageiro" por defeito

            if (string.IsNullOrEmpty(nomeCliente) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Preencha o nome do cliente e a senha.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 1. Verificar se o Nome do Cliente (username) já existe
                using (SqlConnection connCheck = new SqlConnection(connString))
                {
                    using (SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Cliente WHERE Nome = @nomeCliente", connCheck))
                    {
                        cmdCheck.Parameters.AddWithValue("@nomeCliente", nomeCliente);
                        connCheck.Open();
                        int count = (int)cmdCheck.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Este nome de cliente já existe. Por favor, escolha outro.", "Aviso",
                                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                // 2. Inserir o novo cliente
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Inserir na tabela Cliente
                    using (var cmd = new SqlCommand(
                        "INSERT INTO Cliente (Nome, Senha, PerfilId) VALUES (@nomeCliente, @senha, @perfilId)", conn))
                    {
                        cmd.Parameters.AddWithValue("@nomeCliente", nomeCliente);
                        cmd.Parameters.AddWithValue("@senha", password); // Lembre-se de usar hash em produção!
                        cmd.Parameters.AddWithValue("@perfilId", perfilIdPadrao);

                        conn.Open();
                        int linhasAfetadas = cmd.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            MessageBox.Show("Conta de cliente criada com sucesso!", "Sucesso",
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Login form1 = new Login();
                            form1.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Não foi possível criar a conta de cliente.", "Erro",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Invalid object name 'Cliente'") || ex.Message.Contains("Invalid object name 'Perfil'"))
                {
                    MessageBox.Show("Erro crítico: Uma tabela necessária (Cliente ou Perfil) não foi encontrada. Verifique a configuração do banco.", "Erro de Banco de Dados",
                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Erro ao conectar ou gravar no banco: " + ex.Message, "Erro",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Métodos não utilizados do seu ficheiro original
        private void guna2PictureBox1_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
    }
}