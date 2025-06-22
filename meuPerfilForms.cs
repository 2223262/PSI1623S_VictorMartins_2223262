using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class meuPerfilForms : Form
    {
        private string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        private int clienteId = Login.ClienteLogadoId;

        public meuPerfilForms()
        {
            InitializeComponent();
            this.Load += meuPerfilForms_Load;
        }

        private void meuPerfilForms_Load(object sender, EventArgs e)
        {
            this.Text = "Gestão de Perfil";
            CarregarDadosPerfil();
        }

        private void CarregarDadosPerfil()
        {
            if (clienteId <= 0)
            {
                MessageBox.Show("Não foi possível identificar o utilizador.", "Erro de Autenticação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            string query = @"SELECT c.Nome, c.Senha, p.Descricao AS Perfil 
                             FROM Cliente c 
                             JOIN Perfil p ON c.PerfilId = p.Id 
                             WHERE c.Id = @clienteId";

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        label3.Text = reader["Nome"].ToString();
                        label5.Text = reader["Perfil"].ToString();

                        guna2TextBox1.Text = reader["Senha"].ToString();
                        guna2TextBox1.ReadOnly = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar os dados do perfil: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
           
        }
        private void guna2Separator1_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void guna2TextBox1_TextChanged(object sender, EventArgs e) { }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2Button2_Click_1(object sender, EventArgs e)
        {
            string novaSenha = guna2TextBox2.Text;
            string confirmarNovaSenha = guna2TextBox3.Text;

            if (string.IsNullOrWhiteSpace(novaSenha) && string.IsNullOrWhiteSpace(confirmarNovaSenha))
            {
                MessageBox.Show("Nenhuma nova senha foi introduzida. Nenhuma alteração foi feita.", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // --- Validações ---
            if (string.IsNullOrWhiteSpace(novaSenha) || string.IsNullOrWhiteSpace(confirmarNovaSenha))
            {
                MessageBox.Show("Para alterar a senha, os campos 'Nova Senha' e 'Confirmar Nova Senha' devem ser preenchidos.", "Campos em Falta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (novaSenha.Length < 4)
            {
                MessageBox.Show("A nova senha deve ter pelo menos 4 caracteres.", "Senha Inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (novaSenha != confirmarNovaSenha)
            {
                MessageBox.Show("A 'Nova Senha' e a 'Confirmação da Nova Senha' não correspondem.", "Erro de Confirmação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- Lógica de Alteração ---
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("UPDATE Cliente SET Senha = @novaSenha WHERE Id = @clienteId", conn))
                {
                    cmd.Parameters.AddWithValue("@novaSenha", novaSenha);
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Senha alterada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro ao atualizar a senha: " + ex.Message, "Erro de Base de Dados", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}