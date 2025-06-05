using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class reciboForms : Form
    {
        private int _reservaId;
        private string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        private Random random = new Random(); // Para gerar horas aleatórias

        // Construtor que recebe o ID da reserva
        public reciboForms(int reservaId)
        {
            InitializeComponent();
            _reservaId = reservaId;
            this.Load += ReciboForms_Load; // Associa o evento Load
        }

        // Construtor padrão para o designer
        public reciboForms()
        {
            InitializeComponent();
        }

        private void ReciboForms_Load(object sender, EventArgs e)
        {
            if (_reservaId > 0)
            {
                CarregarDadosNosLabels();
            }
        }

        private void CarregarDadosNosLabels()
        {
            // Começa por esconder todos os labels relacionados com a viagem de volta
            // NOTA: Você também precisará esconder os títulos estáticos (ex: um label com o texto "Origem Volta:")
            lblOrigemVolta.Visible = false;
            lblDestinoVolta.Visible = false;
            lblDataVolta.Visible = false;
            lblHrVooVolta.Visible = false;
            lblClasseVolta.Visible = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = @"
                        SELECT
                            c.Nome AS NomeCliente,
                            v.PaisOrigem, v.CidadeOrigem,
                            v.PaisDestino, v.CidadeDestino,
                            v.DataHora AS DataViagem,
                            v.PrecoBase,
                            r.Classe,
                            r.DataReserva AS DataCompra,
                            r.DataRetorno
                        FROM Reserva r
                        JOIN Cliente c ON r.ClienteId = c.Id
                        JOIN Voo v ON r.VooId = v.Id
                        WHERE r.Id = @reservaId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@reservaId", _reservaId);
                        conn.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            // --- Preencher Labels Gerais e de IDA ---

                            // Cabeçalho e Informações Gerais
                            lblNomeUtilizador.Text = reader["NomeCliente"].ToString();
                            DateTime dataCompra = Convert.ToDateTime(reader["DataCompra"]);
                            lblDataCompra.Text = dataCompra.ToString("dd/MM/yyyy HH:mm"); // Hora real da compra
                            lblDataCabecario.Text = dataCompra.ToString("dd/MM/yyyy"); // Apenas data para o cabeçalho

                            // Informações do Voo de IDA
                            lblOrigem.Text = $"{reader["CidadeOrigem"]}, {reader["PaisOrigem"]}";
                            lblDestino.Text = $"{reader["CidadeDestino"]}, {reader["PaisDestino"]}";
                            lblData.Text = Convert.ToDateTime(reader["DataViagem"]).ToString("dd/MM/yyyy");
                            lblClasse.Text = reader["Classe"].ToString();

                            // Gerar hora aleatória para o voo de ida
                            TimeSpan horaVooIda = new TimeSpan(random.Next(8, 22), random.Next(0, 12) * 5, 0);
                            lblHrVoo.Text = horaVooIda.ToString(@"hh\:mm");

                            // Total
                            lblTotal.Text = $"{Convert.ToDecimal(reader["PrecoBase"]):C}"; // Formata como moeda (€)

                            // --- Preencher Labels de VOLTA (se houver) ---
                            if (reader["DataRetorno"] != DBNull.Value)
                            {
                                // Torna os labels de volta visíveis
                                lblOrigemVolta.Visible = true;
                                lblDestinoVolta.Visible = true;
                                lblDataVolta.Visible = true;
                                lblHrVooVolta.Visible = true;
                                lblClasseVolta.Visible = true;

                                // Preenche os dados da volta (origem e destino invertidos)
                                lblOrigemVolta.Text = $"{reader["CidadeDestino"]}, {reader["PaisDestino"]}";
                                lblDestinoVolta.Text = $"{reader["CidadeOrigem"]}, {reader["PaisOrigem"]}";
                                lblDataVolta.Text = Convert.ToDateTime(reader["DataRetorno"]).ToString("dd/MM/yyyy");
                                lblClasseVolta.Text = reader["Classe"].ToString();

                                // Gerar hora aleatória para o voo de volta
                                TimeSpan horaVooVolta = new TimeSpan(random.Next(8, 22), random.Next(0, 12) * 5, 0);
                                lblHrVooVolta.Text = horaVooVolta.ToString(@"hh\:mm");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Não foi possível encontrar os dados da reserva.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro ao carregar os dados do recibo: " + ex.Message, "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void reciboForms_Load(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flowLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flowLayoutPanelRecibo_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2PictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox4_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void lblDestinoVolta_Click(object sender, EventArgs e)
        {

        }
    }
}
