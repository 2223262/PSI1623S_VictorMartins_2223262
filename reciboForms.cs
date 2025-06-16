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
using System.IO;

namespace _DigiAirlines
{
    public partial class reciboForms : Form
    {
        // --- Variáveis para armazenar os dados do recibo ---
        private int _reservaId;
        private string _nomeCliente;
        private DateTime _dataCompra;
        private decimal _precoTotal;
        private string _classe;

        // Dados da Ida
        private string _origemIda;
        private string _destinoIda;
        private DateTime _dataViagemIda;
        private string _horaVooIda;

        // Dados da Volta
        private bool _temViagemDeVolta = false;
        private string _origemVolta;
        private string _destinoVolta;
        private DateTime _dataViagemVolta;
        private string _horaVooVolta;

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
                CarregarEExibirDados();
            }
        }

        private decimal CalcularPrecoComClasse(decimal precoBase, string classe)
        {
            switch (classe)
            {
                case "Executiva": return precoBase * 1.25m;
                case "Primeira-classe": return precoBase * 1.50m;
                default: return precoBase;
            }
        }

        private void CarregarEExibirDados()
        {
            // Esconder controlos da volta por defeito
            lblOrigemVolta.Visible = false;
            lblDestinoVolta.Visible = false;
            lblDataVolta.Visible = false;
            lblHrVooVolta.Visible = false;
            lblClasseVolta.Visible = false;
            // guna2PictureBox3.Visible = false;
            // guna2CirclePictureBox2.Visible = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = @"
                        SELECT c.Nome AS NomeCliente, v.PaisOrigem, v.CidadeOrigem, v.PaisDestino, v.CidadeDestino,
                               v.DataHora AS DataViagem, r.Classe, r.DataReserva AS DataCompra, r.DataRetorno,
                               DestinoIda.Preco AS PrecoIda, DestinoVolta.Preco AS PrecoVolta
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
                            // --- Armazenar todos os dados nas variáveis da classe ---
                            _nomeCliente = reader["NomeCliente"].ToString();
                            _dataCompra = Convert.ToDateTime(reader["DataCompra"]);
                            _classe = reader["Classe"].ToString();

                            // Dados da Ida
                            _origemIda = $"{reader["CidadeOrigem"]}, {reader["PaisOrigem"]}";
                            _destinoIda = $"{reader["CidadeDestino"]}, {reader["PaisDestino"]}";
                            _dataViagemIda = Convert.ToDateTime(reader["DataViagem"]);
                            _horaVooIda = new TimeSpan(random.Next(8, 22), random.Next(0, 12) * 5, 0).ToString(@"hh\:mm");

                            decimal precoBaseIda = reader["PrecoIda"] != DBNull.Value ? Convert.ToDecimal(reader["PrecoIda"]) : 0;
                            _precoTotal = CalcularPrecoComClasse(precoBaseIda, _classe);

                            // Dados da Volta (se existir)
                            if (reader["DataRetorno"] != DBNull.Value)
                            {
                                _temViagemDeVolta = true;
                                _origemVolta = _destinoIda; // Origem da volta é o destino da ida
                                _destinoVolta = _origemIda; // Destino da volta é a origem da ida
                                _dataViagemVolta = Convert.ToDateTime(reader["DataRetorno"]);
                                _horaVooVolta = new TimeSpan(random.Next(8, 22), random.Next(0, 12) * 5, 0).ToString(@"hh\:mm");

                                decimal precoBaseVolta = reader["PrecoVolta"] != DBNull.Value ? Convert.ToDecimal(reader["PrecoVolta"]) : 0;
                                _precoTotal += CalcularPrecoComClasse(precoBaseVolta, _classe);
                            }

                            // --- Agora, preencher os labels com os dados armazenados ---
                            PreencherLabels();
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

        private void PreencherLabels()
        {
            lblNomeUtilizador.Text = _nomeCliente;
            lblDataCompra.Text = _dataCompra.ToString("dd/MM/yyyy HH:mm");
            lblDataCabecario.Text = _dataCompra.ToString("dd/MM/yyyy");

            lblOrigem.Text = _origemIda;
            lblDestino.Text = _destinoIda;
            lblData.Text = _dataViagemIda.ToString("dd/MM/yyyy");
            lblClasse.Text = _classe;
            lblHrVoo.Text = _horaVooIda;

            if (_temViagemDeVolta)
            {
                lblOrigemVolta.Visible = true;
                lblDestinoVolta.Visible = true;
                lblDataVolta.Visible = true;
                lblHrVooVolta.Visible = true;
                lblClasseVolta.Visible = true;
                // guna2PictureBox3.Visible = true;
                // guna2CirclePictureBox2.Visible = true;

                lblOrigemVolta.Text = _origemVolta;
                lblDestinoVolta.Text = _destinoVolta;
                lblDataVolta.Text = _dataViagemVolta.ToString("dd/MM/yyyy");
                lblClasseVolta.Text = _classe;
                lblHrVooVolta.Text = _horaVooVolta;
            }

            CultureInfo culturaEuro = new CultureInfo("pt-PT");
            lblTotal.Text = _precoTotal.ToString("C", culturaEuro);
        }

        // --- NOVO MÉTODO PARA O BOTÃO ---
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Ficheiro de Texto|*.txt";
                saveFileDialog.Title = "Salvar Recibo da Reserva";
                saveFileDialog.FileName = $"Recibo_Reserva_{_reservaId}.txt"; // Sugere um nome de ficheiro

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Constrói o conteúdo do ficheiro de texto
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("*************************************");
                        sb.AppendLine("* RECIBO - DIGIAIRLINES        *");
                        sb.AppendLine("*************************************");
                        sb.AppendLine();
                        sb.AppendLine($"Cliente: {_nomeCliente}");
                        sb.AppendLine($"Data da Compra: {_dataCompra:dd/MM/yyyy HH:mm}");
                        sb.AppendLine();
                        sb.AppendLine("-------------------------------------");
                        sb.AppendLine("          BILHETE DE IDA");
                        sb.AppendLine("-------------------------------------");
                        sb.AppendLine($"Origem:      {_origemIda}");
                        sb.AppendLine($"Destino:     {_destinoIda}");
                        sb.AppendLine($"Data:        {_dataViagemIda:dd/MM/yyyy}");
                        sb.AppendLine($"Hora do Voo: {_horaVooIda}");
                        sb.AppendLine($"Classe:      {_classe}");
                        sb.AppendLine();

                        if (_temViagemDeVolta)
                        {
                            sb.AppendLine("-------------------------------------");
                            sb.AppendLine("          BILHETE DE VOLTA");
                            sb.AppendLine("-------------------------------------");
                            sb.AppendLine($"Origem:      {_origemVolta}");
                            sb.AppendLine($"Destino:     {_destinoVolta}");
                            sb.AppendLine($"Data:        {_dataViagemVolta:dd/MM/yyyy}");
                            sb.AppendLine($"Hora do Voo: {_horaVooVolta}");
                            sb.AppendLine($"Classe:      {_classe}");
                            sb.AppendLine();
                        }

                        sb.AppendLine("=====================================");
                        sb.AppendLine($"TOTAL: {_precoTotal.ToString("C", new CultureInfo("pt-PT"))}");
                        sb.AppendLine("=====================================");
                        sb.AppendLine();
                        sb.AppendLine("Obrigado por voar com a DigiAirlines!");

                        // Grava o conteúdo no ficheiro escolhido pelo utilizador
                        File.WriteAllText(saveFileDialog.FileName, sb.ToString());

                        MessageBox.Show("Recibo salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ocorreu um erro ao salvar o ficheiro: " + ex.Message, "Erro de Gravação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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
