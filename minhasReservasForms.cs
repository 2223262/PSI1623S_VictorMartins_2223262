using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace _DigiAirlines
{
    public partial class minhasReservasForms : Form
    {
        private string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        private int clienteId = Login.ClienteLogadoId;

        public minhasReservasForms()
        {
            InitializeComponent();
            this.Load += MinhasReservasForms_Load;
        }

        private void MinhasReservasForms_Load(object sender, EventArgs e)
        {
            this.Text = "Minhas Reservas - DigiAirlines";
            CarregarReservasNoPainel();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void CarregarReservasNoPainel()
        {
            // Código para carregar as reservas no FlowLayoutPanel (sem alterações)
            #region Código do CarregarReservasNoPainel
            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.WrapContents = false;

            Label lblTitulo = new Label { Text = "O Meu Histórico de Reservas", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64), AutoSize = true, Margin = new Padding(10, 10, 10, 20) };
            flowLayoutPanel1.Controls.Add(lblTitulo);

            string query = @"SELECT r.Id AS ReservaID, v.CidadeOrigem, v.PaisOrigem, v.CidadeDestino, v.PaisDestino, v.DataHora AS DataViagem, r.Classe FROM Reserva r JOIN Voo v ON r.VooId = v.Id WHERE r.ClienteId = @clienteId ORDER BY v.DataHora DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        flowLayoutPanel1.Controls.Add(new Label { Text = "Você ainda não tem nenhuma reserva.", Font = new Font("Segoe UI", 12), AutoSize = true, Margin = new Padding(10) });
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            Guna2Panel painelReserva = CriarPainelDeReserva(
                                Convert.ToInt32(reader["ReservaID"]),
                                $"{reader["CidadeOrigem"]}, {reader["PaisOrigem"]}",
                                $"{reader["CidadeDestino"]}, {reader["PaisDestino"]}",
                                Convert.ToDateTime(reader["DataViagem"]),
                                reader["Classe"].ToString()
                            );
                            flowLayoutPanel1.Controls.Add(painelReserva);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar o histórico de reservas: " + ex.Message, "Erro de Base de Dados", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            #endregion
        }

        private Guna2Panel CriarPainelDeReserva(int reservaId, string origem, string destino, DateTime dataViagem, string classe)
        {
            // Código para criar os painéis dinâmicos (sem alterações)
            #region Código do CriarPainelDeReserva
            Guna2Panel painel = new Guna2Panel
            {
                Width = flowLayoutPanel1.ClientSize.Width - 25,
                Height = 120,
                Margin = new Padding(10, 0, 10, 10),
                BorderRadius = 10,
                FillColor = Color.WhiteSmoke,
                BorderThickness = 1,
                BorderColor = Color.LightGray
            };

            Label lblDestino = new Label { Text = $"Destino: {destino}", Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Location = new Point(15, 15) };
            Label lblOrigem = new Label { Text = $"Origem: {origem}", Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(15, 45) };
            Label lblData = new Label { Text = $"Data: {dataViagem:dd/MM/yyyy}", Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(15, 65) };
            Label lblClasse = new Label { Text = $"Classe: {classe}", Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(15, 85) };

            Guna2Button btnCancelar = new Guna2Button
            {
                Text = "Cancelar",
                Tag = new Tuple<int, DateTime>(reservaId, dataViagem),
                Width = 120,
                Height = 35,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(painel.Width - 135, painel.Height - 50),
                FillColor = Color.IndianRed,
                ForeColor = Color.White,
                BorderRadius = 5
            };
            btnCancelar.Click += BtnCancelar_Click;

            Guna2Button btnEditar = new Guna2Button
            {
                Text = "Editar",
                Tag = new Tuple<int, DateTime>(reservaId, dataViagem),
                Width = 120,
                Height = 35,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(btnCancelar.Location.X - 130, btnCancelar.Location.Y),
                FillColor = Color.CornflowerBlue,
                ForeColor = Color.White,
                BorderRadius = 5
            };
            btnEditar.Click += btnEditarReserva_Click;

            painel.Controls.Add(lblDestino);
            painel.Controls.Add(lblOrigem);
            painel.Controls.Add(lblData);
            painel.Controls.Add(lblClasse);
            painel.Controls.Add(btnCancelar);
            painel.Controls.Add(btnEditar);

            return painel;
            #endregion
        }

        // --- MÉTODO 'EDITAR' ATUALIZADO (SEM MENSAGENS DE DEBUG) ---
        private void btnEditarReserva_Click(object sender, EventArgs e)
        {
            Guna2Button botaoClicado = sender as Guna2Button;
            if (botaoClicado == null) return;

            var tagInfo = (Tuple<int, DateTime>)botaoClicado.Tag;
            int reservaId = tagInfo.Item1;
            DateTime dataViagem = tagInfo.Item2;

            if (dataViagem.Date < DateTime.Today)
            {
                MessageBox.Show("Não é possível editar uma reserva para uma viagem que já ocorreu.", "Ação Inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Abre o formulário de edição e espera que ele feche
            using (EditarReservaForm formEditar = new EditarReservaForm(reservaId))
            {
                var resultado = formEditar.ShowDialog();

                // Se a edição foi confirmada, atualiza a lista de reservas
                if (resultado == DialogResult.OK)
                {
                    CarregarReservasNoPainel();
                }
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            // Código do botão Cancelar (sem alterações)
            #region Código do BtnCancelar_Click
            Guna2Button botaoClicado = sender as Guna2Button;
            if (botaoClicado == null) return;

            var tagInfo = (Tuple<int, DateTime>)botaoClicado.Tag;
            int reservaIdParaCancelar = tagInfo.Item1;
            DateTime dataViagem = tagInfo.Item2;

            if (dataViagem.Date < DateTime.Today)
            {
                MessageBox.Show("Não é possível cancelar uma reserva para uma viagem que já ocorreu.", "Ação Inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Tem a certeza que deseja cancelar esta reserva permanentemente?", "Confirmar Cancelamento", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult != DialogResult.Yes) return;

            string query = "DELETE FROM Reserva WHERE Id = @reservaId AND ClienteId = @clienteId";

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@reservaId", reservaIdParaCancelar);
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    conn.Open();
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Reserva cancelada com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CarregarReservasNoPainel();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro ao cancelar a reserva: " + ex.Message, "Erro de Base de Dados", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            #endregion
        }
    }
}