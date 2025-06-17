using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class adminForms : Form
    {
        private string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";

        public adminForms()
        {
            InitializeComponent();
            this.Text = "Painel de Administração - DigiAirlines";
        }

        private void adminForms_Load(object sender, EventArgs e)
        {
            // Ao carregar, podemos mostrar uma mensagem de boas-vindas ou as estatísticas iniciais
            guna2Button3_Click(sender, e); // Mostra as estatísticas por defeito
        }

        // --- MÉTODOS PARA CARREGAR OS DADOS ---

        private void CarregarClientes()
        {
            // Limpa o painel e adiciona um título
            flowLayoutPanel1.Controls.Clear();
            AdicionarTituloAoPainel("Gestão de Clientes");

            // Cria uma tabela para exibir os dados
            DataGridView dgv = CriarDataGridView();

            // Query para buscar todos os clientes e o nome do seu perfil
            string query = @"SELECT c.Id, c.Nome, p.Descricao AS Perfil 
                             FROM Cliente c 
                             JOIN Perfil p ON c.PerfilId = p.Id 
                             ORDER BY c.Nome";

            // Preenche a tabela com os dados
            dgv.DataSource = ExecutarQuery(query);

            // Adiciona a tabela ao painel
            flowLayoutPanel1.Controls.Add(dgv);
        }

        private void CarregarDestinos()
        {
            flowLayoutPanel1.Controls.Clear();
            AdicionarTituloAoPainel("Gestão de Destinos");

            DataGridView dgv = CriarDataGridView();
            string query = "SELECT Id, Pais, Cidade, Preco FROM Destino ORDER BY Pais, Cidade";
            dgv.DataSource = ExecutarQuery(query);
            flowLayoutPanel1.Controls.Add(dgv);
        }

        private void CarregarReservas()
        {
            flowLayoutPanel1.Controls.Clear();
            AdicionarTituloAoPainel("Histórico de Todas as Reservas");

            DataGridView dgv = CriarDataGridView();
            string query = @"SELECT r.Id AS ReservaID, c.Nome AS Cliente, 
                                    v.CidadeOrigem + ', ' + v.PaisOrigem AS Origem,
                                    v.CidadeDestino + ', ' + v.PaisDestino AS Destino,
                                    r.DataReserva
                             FROM Reserva r
                             JOIN Cliente c ON r.ClienteId = c.Id
                             JOIN Voo v ON r.VooId = v.Id
                             ORDER BY r.DataReserva DESC";
            dgv.DataSource = ExecutarQuery(query);
            flowLayoutPanel1.Controls.Add(dgv);
        }

        private void CarregarEstatisticas()
        {
            flowLayoutPanel1.Controls.Clear();
            AdicionarTituloAoPainel("Estatísticas Rápidas");

            // Querys para obter os totais
            string totalClientesQuery = "SELECT COUNT(*) FROM Cliente";
            string totalReservasQuery = "SELECT COUNT(*) FROM Reserva";
            string totalDestinosQuery = "SELECT COUNT(*) FROM Destino";

            // Executa as querys e obtém os resultados
            string totalClientes = ExecutarQueryScalar(totalClientesQuery);
            string totalReservas = ExecutarQueryScalar(totalReservasQuery);
            string totalDestinos = ExecutarQueryScalar(totalDestinosQuery);

            // Cria e adiciona os labels de estatísticas ao painel
            flowLayoutPanel1.Controls.Add(CriarLabelEstatistica($"Total de Clientes Registados: {totalClientes}"));
            flowLayoutPanel1.Controls.Add(CriarLabelEstatistica($"Total de Reservas Efetuadas: {totalReservas}"));
            flowLayoutPanel1.Controls.Add(CriarLabelEstatistica($"Total de Destinos Disponíveis: {totalDestinos}"));
        }

        // --- MÉTODOS AUXILIARES ---

        // Método para executar uma query e retornar uma tabela de dados (DataTable)
        private DataTable ExecutarQuery(string query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            sda.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao aceder à base de dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // Método para executar uma query que retorna um único valor
        private string ExecutarQueryScalar(string query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result?.ToString() ?? "0";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao aceder à base de dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "Erro";
            }
        }

        // Método para criar e configurar uma DataGridView padrão
        private DataGridView CriarDataGridView()
        {
            DataGridView dgv = new DataGridView();
            dgv.Width = flowLayoutPanel1.ClientSize.Width - 10;
            dgv.Height = 300;
            dgv.BackgroundColor = Color.White;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            return dgv;
        }

        // Método para criar um Label de título padrão
        private void AdicionarTituloAoPainel(string titulo)
        {
            Label lblTitulo = new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                AutoSize = true,
                Margin = new Padding(5, 5, 5, 10)
            };
            flowLayoutPanel1.Controls.Add(lblTitulo);
        }

        // Método para criar um Label de estatística padrão
        private Label CriarLabelEstatistica(string texto)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Margin = new Padding(10, 5, 10, 5)
            };
        }


        // --- EVENTOS DE CLIQUE DOS BOTÕES ---

        private void guna2ButtonGerirClientes_Click(object sender, EventArgs e)
        {
            CarregarClientes();
        }

        private void guna2Button1_Click(object sender, EventArgs e) // Gerir Destinos
        {
            CarregarDestinos();
        }

        private void guna2Button2_Click(object sender, EventArgs e) // Ver Reservas
        {
            CarregarReservas();
        }

        private void guna2Button3_Click(object sender, EventArgs e) // Estatisticas
        {
            CarregarEstatisticas();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Pode ser deixado em branco
        }

        private void adminForms_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {

        }
    }
}