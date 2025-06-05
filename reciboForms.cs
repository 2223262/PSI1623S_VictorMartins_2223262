using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
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
        private Random random = new Random();

        public reciboForms(int reservaId)
        {
            InitializeComponent();
            _reservaId = reservaId;
            this.Load += ReciboForms_Load;
        }

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

        // NOVO: Método auxiliar para calcular o preço final com base na classe
        private decimal CalcularPrecoComClasse(decimal precoBase, string classe)
        {
            switch (classe)
            {
                case "Executiva":
                    return precoBase * 1.25m; // Aumento de 25%
                case "Primeira-classe":
                    return precoBase * 1.50m; // Aumento de 50%
                case "Econômica":
                default:
                    return precoBase; // Sem alteração de preço
            }
        }

        private void CarregarDadosNosLabels()
        {
            // Esconder controlos da volta por defeito
            lblOrigemVolta.Visible = false;
            lblDestinoVolta.Visible = false;
            lblDataVolta.Visible = false;
            lblHrVooVolta.Visible = false;
            lblClasseVolta.Visible = false;
            label3.Visible = false;
            // Se tiver os PictureBoxes, adicione aqui as linhas para os esconder também
            // guna2PictureBox3.Visible = false;
            // guna2CirclePictureBox2.Visible = false;

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
                            r.Classe,
                            r.DataReserva AS DataCompra,
                            r.DataRetorno,
                            DestinoIda.Preco AS PrecoIda,
                            DestinoVolta.Preco AS PrecoVolta
                        FROM Reserva r
                        JOIN Cliente c ON r.ClienteId = c.Id
                        JOIN Voo v ON r.VooId = v.Id
                        LEFT JOIN Destino AS DestinoIda ON v.PaisDestino = DestinoIda.Pais AND v.CidadeDestino = DestinoIda.Cidade
                        LEFT JOIN Destino AS DestinoVolta ON v.PaisOrigem = DestinoVolta.Pais AND v.CidadeOrigem = DestinoVolta.Cidade
                        WHERE r.Id = @reservaId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@reservaId", _reservaId);
                        conn.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            string classeViagem = reader["Classe"].ToString();

                            // Preencher Labels Gerais e de IDA
                            lblNomeUtilizador.Text = reader["NomeCliente"].ToString();
                            DateTime dataCompra = Convert.ToDateTime(reader["DataCompra"]);
                            lblDataCompra.Text = dataCompra.ToString("dd/MM/yyyy HH:mm");
                            lblDataCabecario.Text = dataCompra.ToString("dd/MM/yyyy");

                            lblOrigem.Text = $"{reader["CidadeOrigem"]}, {reader["PaisOrigem"]}";
                            lblDestino.Text = $"{reader["CidadeDestino"]}, {reader["PaisDestino"]}";
                            lblData.Text = Convert.ToDateTime(reader["DataViagem"]).ToString("dd/MM/yyyy");
                            lblClasse.Text = classeViagem;

                            TimeSpan horaVooIda = new TimeSpan(random.Next(8, 22), random.Next(0, 12) * 5, 0);
                            lblHrVoo.Text = horaVooIda.ToString(@"hh\:mm");

                            // --- LÓGICA DE PREÇO ATUALIZADA ---
                            decimal precoBaseIda = reader["PrecoIda"] != DBNull.Value ? Convert.ToDecimal(reader["PrecoIda"]) : 0;
                            decimal precoFinalIda = CalcularPrecoComClasse(precoBaseIda, classeViagem);
                            decimal precoTotal = precoFinalIda;

                            // Preencher Labels de VOLTA (se houver)
                            if (reader["DataRetorno"] != DBNull.Value)
                            {
                                // Torna os controlos de volta visíveis
                                lblOrigemVolta.Visible = true;
                                lblDestinoVolta.Visible = true;
                                lblDataVolta.Visible = true;
                                lblHrVooVolta.Visible = true;
                                lblClasseVolta.Visible = true;
                                label3.Visible = true;
                                // Se tiver os PictureBoxes, adicione aqui as linhas para os mostrar
                                // guna2PictureBox3.Visible = true;
                                // guna2CirclePictureBox2.Visible = true;

                                // Preenche os dados da volta
                                lblOrigemVolta.Text = $"{reader["CidadeDestino"]}, {reader["PaisDestino"]}";
                                lblDestinoVolta.Text = $"{reader["CidadeOrigem"]}, {reader["PaisOrigem"]}";
                                lblDataVolta.Text = Convert.ToDateTime(reader["DataRetorno"]).ToString("dd/MM/yyyy");
                                lblClasseVolta.Text = classeViagem;

                                TimeSpan horaVooVolta = new TimeSpan(random.Next(8, 22), random.Next(0, 12) * 5, 0);
                                lblHrVooVolta.Text = horaVooVolta.ToString(@"hh\:mm");

                                // Soma o preço da volta (já com o ajuste da classe) ao total
                                decimal precoBaseVolta = reader["PrecoVolta"] != DBNull.Value ? Convert.ToDecimal(reader["PrecoVolta"]) : 0;
                                decimal precoFinalVolta = CalcularPrecoComClasse(precoBaseVolta, classeViagem);
                                precoTotal += precoFinalVolta;
                            }

                            // Formata e exibe o preço total em Euros
                            CultureInfo culturaEuro = new CultureInfo("pt-PT");
                            lblTotal.Text = precoTotal.ToString("C", culturaEuro);
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
