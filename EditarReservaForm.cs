using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class EditarReservaForm : Form
    {
        private string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        private int reservaId;
        private int vooId;
        private decimal custoOriginal = 0;
        private string classeOriginal = "";

        public EditarReservaForm(int idReserva)
        {
            InitializeComponent();
            this.reservaId = idReserva;
            this.Load += EditarReservaForm_Load;
        }

        private void EditarReservaForm_Load(object sender, EventArgs e)
        {
            CarregarDadosReserva();
        }

        private void CarregarDadosReserva()
        {
            try
            {
                string query = @"SELECT r.VooId, r.Classe, v.CidadeOrigem, v.PaisOrigem, v.CidadeDestino, v.PaisDestino, 
                                        v.DataHora AS DataViagem, DestinoIda.Preco AS PrecoIda, DestinoVolta.Preco AS PrecoVolta, r.DataRetorno
                                 FROM Reserva r JOIN Voo v ON r.VooId = v.Id
                                 LEFT JOIN Destino AS DestinoIda ON v.PaisDestino = DestinoIda.Pais AND v.CidadeDestino = DestinoIda.Cidade
                                 LEFT JOIN Destino AS DestinoVolta ON v.PaisOrigem = DestinoVolta.Pais AND v.CidadeOrigem = DestinoVolta.Cidade
                                 WHERE r.Id = @reservaId";

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@reservaId", this.reservaId);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        vooId = Convert.ToInt32(reader["VooId"]);
                        classeOriginal = reader["Classe"].ToString();

                        // Usa os nomes dos controlos do seu designer
                        label3.Text = $"Voo: {reader["CidadeOrigem"]} -> {reader["CidadeDestino"]}";

                        guna2ComboBox1.Items.Clear();
                        guna2ComboBox1.Items.AddRange(new object[] { "Econômica", "Executiva", "Primeira-classe" });
                        guna2ComboBox1.SelectedItem = classeOriginal;

                        guna2DateTimePicker1.Value = Convert.ToDateTime(reader["DataViagem"]);
                        guna2DateTimePicker1.MinDate = DateTime.Today;

                        // Se você tiver um label para a taxa (ex: label4), pode descomentar a linha abaixo
                        // decimal taxaEdicao = custoOriginal * 0.70m;
                        // label4.Text = $"Taxa de Edição: {taxaEdicao.ToString("C", new CultureInfo("pt-PT"))}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar detalhes da reserva: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        // Evento para o botão "Confirmar Alterações"
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            
        }

        // Evento para o botão "Cancelar"
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            if (guna2ComboBox1.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecione uma nova classe.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string novaClasse = guna2ComboBox1.SelectedItem.ToString();
            DateTime novaData = guna2DateTimePicker1.Value.Date;

            // Vamos recalcular o custo e a taxa antes de confirmar
            try
            {
                // Recalcula o custo original para ter certeza
                string queryPreco = @"SELECT DestinoIda.Preco AS PrecoIda, DestinoVolta.Preco AS PrecoVolta, r.DataRetorno
                                      FROM Reserva r JOIN Voo v ON r.VooId = v.Id
                                      LEFT JOIN Destino AS DestinoIda ON v.PaisDestino = DestinoIda.Pais AND v.CidadeDestino = DestinoIda.Cidade
                                      LEFT JOIN Destino AS DestinoVolta ON v.PaisOrigem = DestinoVolta.Pais AND v.CidadeOrigem = DestinoVolta.Cidade
                                      WHERE r.Id = @reservaId";
                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand(queryPreco, conn))
                {
                    cmd.Parameters.AddWithValue("@reservaId", this.reservaId);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        decimal precoBaseIda = reader["PrecoIda"] != DBNull.Value ? Convert.ToDecimal(reader["PrecoIda"]) : 0;
                        custoOriginal = CalcularPrecoComClasse(precoBaseIda, classeOriginal);
                        if (reader["DataRetorno"] != DBNull.Value)
                        {
                            decimal precoBaseVolta = reader["PrecoVolta"] != DBNull.Value ? Convert.ToDecimal(reader["PrecoVolta"]) : 0;
                            custoOriginal += CalcularPrecoComClasse(precoBaseVolta, classeOriginal);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possível calcular a taxa de edição: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            decimal taxaEdicao = custoOriginal * 0.70m;
            string mensagem = $"Confirma a alteração da sua reserva?\nSerá aplicada uma taxa de {taxaEdicao.ToString("C", new CultureInfo("pt-PT"))}.";

            var confirmResult = MessageBox.Show(mensagem, "Confirmar Alteração", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (confirmResult != DialogResult.Yes) return;

            string queryUpdateReserva = "UPDATE Reserva SET Classe = @novaClasse WHERE Id = @reservaId";
            string queryUpdateVoo = "UPDATE Voo SET DataHora = @novaData WHERE Id = @vooId";

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmdVoo = new SqlCommand(queryUpdateVoo, conn))
                    {
                        cmdVoo.Parameters.AddWithValue("@novaData", novaData);
                        cmdVoo.Parameters.AddWithValue("@vooId", this.vooId);
                        cmdVoo.ExecuteNonQuery();
                    }
                    using (SqlCommand cmdReserva = new SqlCommand(queryUpdateReserva, conn))
                    {
                        cmdReserva.Parameters.AddWithValue("@novaClasse", novaClasse);
                        cmdReserva.Parameters.AddWithValue("@reservaId", this.reservaId);
                        cmdReserva.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Reserva atualizada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao gravar as alterações: " + ex.Message, "Erro de Base de Dados", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}