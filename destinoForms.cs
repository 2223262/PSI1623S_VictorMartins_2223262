using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class destinoForms : Form
    {
        string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        int clienteIdParaReserva = Login.ClienteLogadoId; // Usar o ID do Cliente logado

        public destinoForms()
        {
            InitializeComponent();
            label4.Visible = false;
            guna2ComboBox1.Visible = false;
            guna2DateTimePicker1.Visible = false;
            label5.Visible = false;
            searchResult.Visible = false;
            dataGridView1.Visible = false;
        }

        private void destinoForms_Load(object sender, EventArgs e)
        {
            if (clienteIdParaReserva <= 0)
            {
                MessageBox.Show("Erro: ID do cliente não definido. Faça login novamente.", "Erro de Autenticação",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e) // Botão "Confirmar"
        {
            // Validação dos campos do formulário
            if (!txSearch.Text.Contains(" - ") || !guna2TextBox1.Text.Contains(" - "))
            {
                MessageBox.Show("Selecione origem e destino.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(guna2ComboBox1.SelectedItem as string))
            {
                MessageBox.Show("Escolha a classe de assento.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var o = txSearch.Text.Split(new[] { " - " }, StringSplitOptions.None);
            var d = guna2TextBox1.Text.Split(new[] { " - " }, StringSplitOptions.None);
            string po = o[0], co = o[1], pd = d[0], cd = d[1];
            DateTime dataIda = DateTimePicker1.Value.Date;
            DateTime dataRetorno = guna2DateTimePicker1.Value.Date;
            string classe = guna2ComboBox1.SelectedItem as string;

            // Inserção do Voo
            int vooId = 0;
            try
            {
                using (var vooConn = new SqlConnection(connString))
                using (var cmdVoo = vooConn.CreateCommand())
                {
                    cmdVoo.CommandText = @"INSERT INTO Voo (PaisOrigem, CidadeOrigem, PaisDestino, CidadeDestino, DataHora, PrecoBase) VALUES (@po, @co, @pd, @cd, @dh, 0); SELECT SCOPE_IDENTITY();";
                    cmdVoo.Parameters.AddWithValue("@po", po);
                    cmdVoo.Parameters.AddWithValue("@co", co);
                    cmdVoo.Parameters.AddWithValue("@pd", pd);
                    cmdVoo.Parameters.AddWithValue("@cd", cd);
                    cmdVoo.Parameters.AddWithValue("@dh", dataIda);
                    vooConn.Open();
                    vooId = Convert.ToInt32(cmdVoo.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro ao inserir o Voo: " + ex.Message, "Erro Crítico no Voo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (vooId == 0)
            {
                MessageBox.Show("Não foi possível criar o registo do Voo.", "Erro no Voo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Inserção da Reserva (versão limpa, sem mensagens de depuração)
            try
            {
                using (var reservaConn = new SqlConnection(connString))
                using (var cmdReserva = reservaConn.CreateCommand())
                {
                    cmdReserva.CommandText = @"
                        INSERT INTO Reserva (ClienteId, VooId, Classe, Assento, DataReserva, DataRetorno)
                        OUTPUT INSERTED.Id
                        VALUES (@cli, @voo, @classe, @assento, GETDATE(), @retorno);";

                    cmdReserva.Parameters.AddWithValue("@cli", clienteIdParaReserva);
                    cmdReserva.Parameters.AddWithValue("@voo", vooId);
                    cmdReserva.Parameters.AddWithValue("@classe", classe);
                    cmdReserva.Parameters.AddWithValue("@assento", "N/D");

                    if (guna2DateTimePicker1.Visible)
                    {
                        cmdReserva.Parameters.AddWithValue("@retorno", dataRetorno);
                    }
                    else
                    {
                        cmdReserva.Parameters.AddWithValue("@retorno", DBNull.Value);
                    }

                    reservaConn.Open();
                    object result = cmdReserva.ExecuteScalar();
                    int novaReservaId = 0;

                    if (result != null && result != DBNull.Value)
                    {
                        novaReservaId = Convert.ToInt32(result);
                    }

                    if (novaReservaId > 0)
                    {
                        MessageBox.Show("Reserva concluída!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        reciboForms recibo = new reciboForms(novaReservaId);
                        recibo.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Falha ao criar reserva (o ID retornado foi 0 ou nulo).", "Erro de Inserção", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao gravar dados da reserva: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- Resto dos seus métodos de UI ---
        // (txSearch_TextChanged, searchResult_CellContentDoubleClick, etc.)

        private void txSearch_TextChanged(object sender, EventArgs e)
        {
            if (txSearch.TextLength < 2)
            {
                searchResult.Visible = false;
                searchResult.DataSource = null;
                return;
            }

            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand(
                "SELECT Pais, Cidade FROM Destino WHERE Pais LIKE @pais OR Cidade LIKE @cidade", conn))
            {
                cmd.Parameters.AddWithValue("@pais", txSearch.Text + "%");
                cmd.Parameters.AddWithValue("@cidade", txSearch.Text + "%");

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                searchResult.DataSource = dt;
            }
            searchResult.Visible = (searchResult.Rows.Count > 0);
        }

        private void searchResult_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = searchResult.Rows[e.RowIndex];
            txSearch.Text = $"{row.Cells[0].Value} - {row.Cells[1].Value}";
            searchResult.Visible = false;
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (guna2TextBox1.TextLength < 2)
            {
                dataGridView1.Visible = false;
                dataGridView1.DataSource = null;
                return;
            }

            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand(
                "SELECT Pais, Cidade FROM Destino WHERE Pais LIKE @pais OR Cidade LIKE @cidade", conn))
            {
                cmd.Parameters.AddWithValue("@pais", guna2TextBox1.Text + "%");
                cmd.Parameters.AddWithValue("@cidade", guna2TextBox1.Text + "%");

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                dataGridView1.DataSource = dt;
            }
            dataGridView1.Visible = (dataGridView1.Rows.Count > 0);
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dataGridView1.Rows[e.RowIndex];
            guna2TextBox1.Text = $"{row.Cells[0].Value} - {row.Cells[1].Value}";
            dataGridView1.Visible = false;
            label4.Visible = true;
            guna2ComboBox1.Visible = true;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void guna2CustomCheckBox1_Click(object sender, EventArgs e)
        {
            bool marcado = guna2CustomCheckBox1.Checked;
            guna2DateTimePicker1.Visible = marcado;
            label5.Visible = marcado;
        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (DateTimePicker1.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Data inválida! Apenas datas a partir de hoje.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DateTimePicker1.Value = DateTime.Today;
            }
        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (guna2DateTimePicker1.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Data inválida! Apenas datas a partir de hoje.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2DateTimePicker1.Value = DateTime.Today;
            }
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e) { }
        private void searchResult_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
    }
}