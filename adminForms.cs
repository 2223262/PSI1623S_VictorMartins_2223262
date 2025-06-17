using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace _DigiAirlines
{
    public partial class adminForms : Form
    {
        private string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";

        private string secaoAtual = "";
        private DataGridView dgvPrincipal;

        public adminForms()
        {
            InitializeComponent();
            this.Text = "Painel de Administração - DigiAirlines";
        }

        private void adminForms_Load(object sender, EventArgs e)
        {
            // Começa mostrando as estatísticas por defeito (os botões de ação estarão escondidos)
            guna2Button3_Click(sender, e);
        }

        // --- MÉTODOS PARA CARREGAR OS DADOS ---

        private void CarregarClientes()
        {
            secaoAtual = "Clientes";
            DefinirVisibilidadeBotoesAcao(true); // Mostra os botões de ação
            flowLayoutPanel1.Controls.Clear();
            AdicionarTituloAoPainel("Gestão de Clientes");

            dgvPrincipal = CriarDataGridView();
            string query = @"SELECT c.Id, c.Nome, p.Descricao AS Perfil FROM Cliente c JOIN Perfil p ON c.PerfilId = p.Id ORDER BY c.Nome";
            dgvPrincipal.DataSource = ExecutarQuery(query);
            flowLayoutPanel1.Controls.Add(dgvPrincipal);
        }

        private void CarregarDestinos()
        {
            secaoAtual = "Destinos";
            DefinirVisibilidadeBotoesAcao(true); // Mostra os botões de ação
            flowLayoutPanel1.Controls.Clear();
            AdicionarTituloAoPainel("Gestão de Destinos");

            dgvPrincipal = CriarDataGridView();
            string query = "SELECT Id, Pais, Cidade, Preco FROM Destino ORDER BY Pais, Cidade";
            dgvPrincipal.DataSource = ExecutarQuery(query);
            flowLayoutPanel1.Controls.Add(dgvPrincipal);
        }

        private void CarregarReservas()
        {
            secaoAtual = "Reservas";
            DefinirVisibilidadeBotoesAcao(false); // Esconde os botões de ação
            flowLayoutPanel1.Controls.Clear();
            AdicionarTituloAoPainel("Histórico de Todas as Reservas");

            dgvPrincipal = CriarDataGridView();
            string query = @"SELECT r.Id AS ReservaID, c.Nome AS Cliente, 
                                    v.CidadeOrigem + ', ' + v.PaisOrigem AS Origem,
                                    v.CidadeDestino + ', ' + v.PaisDestino AS Destino,
                                    r.DataReserva
                             FROM Reserva r
                             JOIN Cliente c ON r.ClienteId = c.Id
                             JOIN Voo v ON r.VooId = v.Id
                             ORDER BY r.DataReserva DESC";
            dgvPrincipal.DataSource = ExecutarQuery(query);
            flowLayoutPanel1.Controls.Add(dgvPrincipal);
        }

        private void CarregarEstatisticas()
        {
            secaoAtual = "Estatisticas";
            DefinirVisibilidadeBotoesAcao(false); // Esconde os botões de ação
            flowLayoutPanel1.Controls.Clear();
            AdicionarTituloAoPainel("Estatísticas Rápidas");

            string totalClientes = ExecutarQueryScalar("SELECT COUNT(*) FROM Cliente");
            string totalReservas = ExecutarQueryScalar("SELECT COUNT(*) FROM Reserva");
            string totalDestinos = ExecutarQueryScalar("SELECT COUNT(*) FROM Destino");

            flowLayoutPanel1.Controls.Add(CriarLabelEstatistica($"Total de Clientes Registados: {totalClientes}"));
            flowLayoutPanel1.Controls.Add(CriarLabelEstatistica($"Total de Reservas Efetuadas: {totalReservas}"));
            flowLayoutPanel1.Controls.Add(CriarLabelEstatistica($"Total de Destinos Disponíveis: {totalDestinos}"));
        }

        // --- MÉTODOS AUXILIARES ---

        // NOVO: Método para controlar a visibilidade dos botões de ação
        private void DefinirVisibilidadeBotoesAcao(bool visivel)
        {
            guna2Button4.Visible = visivel; // Botão Acrescentar
            guna2Button5.Visible = visivel; // Botão Refresh
            guna2Button6.Visible = visivel; // Botão Apagar
        }

        private DataTable ExecutarQuery(string query)
        {
            try
            {
                using (var conn = new SqlConnection(connString))
                using (var cmd = new SqlCommand(query, conn))
                using (var sda = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    sda.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao aceder à base de dados: " + ex.Message, "Erro de Leitura", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private string ExecutarQueryScalar(string query)
        {
            try
            {
                using (var conn = new SqlConnection(connString))
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao aceder à base de dados: " + ex.Message, "Erro de Leitura", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "Erro";
            }
        }

        private void ExecutarComando(string query, SqlParameter[] parametros = null)
        {
            try
            {
                using (var conn = new SqlConnection(connString))
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (parametros != null)
                    {
                        cmd.Parameters.AddRange(parametros);
                    }
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    MessageBox.Show("Ação bloqueada: Este registo não pode ser apagado porque está a ser utilizado por outras tabelas (ex: um cliente com reservas existentes).", "Erro de Referência", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Erro de base de dados: " + ex.Message, "Erro de Escrita", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro geral: " + ex.Message, "Erro Inesperado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataGridView CriarDataGridView()
        {
            return new DataGridView
            {
                Width = flowLayoutPanel1.ClientSize.Width - 10,
                Height = 350,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
        }

        private void AdicionarTituloAoPainel(string titulo)
        {
            flowLayoutPanel1.Controls.Add(new Label { Text = titulo, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64), AutoSize = true, Margin = new Padding(5, 5, 5, 10) });
        }

        private Label CriarLabelEstatistica(string texto)
        {
            return new Label { Text = texto, Font = new Font("Segoe UI", 12), AutoSize = true, Margin = new Padding(10, 5, 10, 5) };
        }

        // --- EVENTOS DE CLIQUE DOS BOTÕES DE NAVEGAÇÃO ---

        private void guna2ButtonGerirClientes_Click(object sender, EventArgs e) { CarregarClientes(); }
        private void guna2Button1_Click(object sender, EventArgs e) { CarregarDestinos(); }
        private void guna2Button2_Click(object sender, EventArgs e) { CarregarReservas(); }
        private void guna2Button3_Click(object sender, EventArgs e) { CarregarEstatisticas(); }

        // --- EVENTOS DE CLIQUE DOS BOTÕES DE AÇÃO ---

        private void guna2Button4_Click(object sender, EventArgs e) // ACRESCENTAR
        {
            switch (secaoAtual)
            {
                case "Clientes":
                    string nome = Interaction.InputBox("Introduza o nome do novo cliente:", "Acrescentar Cliente");
                    if (string.IsNullOrWhiteSpace(nome)) return;
                    string senha = Interaction.InputBox($"Introduza a senha para '{nome}':", "Acrescentar Cliente");
                    if (string.IsNullOrWhiteSpace(senha)) return;

                    string queryCliente = "INSERT INTO Cliente (Nome, Senha, PerfilId) VALUES (@nome, @senha, 1)";
                    SqlParameter[] paramsCliente = { new SqlParameter("@nome", nome), new SqlParameter("@senha", senha) };
                    ExecutarComando(queryCliente, paramsCliente);
                    CarregarClientes();
                    break;

                case "Destinos":
                    string pais = Interaction.InputBox("Introduza o país do novo destino:", "Acrescentar Destino");
                    if (string.IsNullOrWhiteSpace(pais)) return;
                    string cidade = Interaction.InputBox($"Introduza a cidade para '{pais}':", "Acrescentar Destino");
                    if (string.IsNullOrWhiteSpace(cidade)) return;
                    string precoStr = Interaction.InputBox($"Introduza o preço para '{cidade}':", "Acrescentar Destino");
                    if (!decimal.TryParse(precoStr, out decimal preco))
                    {
                        MessageBox.Show("Preço inválido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string queryDestino = "INSERT INTO Destino (Pais, Cidade, Preco) VALUES (@pais, @cidade, @preco)";
                    SqlParameter[] paramsDestino = { new SqlParameter("@pais", pais), new SqlParameter("@cidade", cidade), new SqlParameter("@preco", preco) };
                    ExecutarComando(queryDestino, paramsDestino);
                    CarregarDestinos();
                    break;

                default:
                    MessageBox.Show("Selecione uma secção (Clientes ou Destinos) para poder acrescentar um novo registo.", "Ação Inválida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void guna2Button5_Click(object sender, EventArgs e) // REFRESH
        {
            switch (secaoAtual)
            {
                case "Clientes": CarregarClientes(); break;
                case "Destinos": CarregarDestinos(); break;
                case "Reservas": CarregarReservas(); break;
                case "Estatisticas": CarregarEstatisticas(); break;
            }
        }

        private void guna2Button6_Click(object sender, EventArgs e) // APAGAR
        {
            if (dgvPrincipal == null || dgvPrincipal.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor, selecione uma linha inteira na tabela para apagar.", "Nenhum Registo Selecionado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Tem a certeza que deseja apagar o registo selecionado permanentemente?", "Confirmar Eliminação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult != DialogResult.Yes) return;

            int idParaApagar = Convert.ToInt32(dgvPrincipal.SelectedRows[0].Cells["Id"].Value);

            switch (secaoAtual)
            {
                case "Clientes":
                    string queryCliente = "DELETE FROM Cliente WHERE Id = @id";
                    ExecutarComando(queryCliente, new[] { new SqlParameter("@id", idParaApagar) });
                    CarregarClientes();
                    break;

                case "Destinos":
                    string queryDestino = "DELETE FROM Destino WHERE Id = @id";
                    ExecutarComando(queryDestino, new[] { new SqlParameter("@id", idParaApagar) });
                    CarregarDestinos();
                    break;

                default:
                    MessageBox.Show("Não é possível apagar registos nesta secção.", "Ação Inválida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void adminForms_FormClosed(object sender, FormClosedEventArgs e) { Application.Exit(); }
    }
}