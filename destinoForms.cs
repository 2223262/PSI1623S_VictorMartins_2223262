using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class destinoForms : Form
    {
        string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        int usuarioId = 1;  // substituir pelo ID do usuário logado

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
            // nada aqui
        }

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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

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

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void searchResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        
        
        }
        private void dataGridiew1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // valida origem/destino
            if (!txSearch.Text.Contains(" - ") || !guna2TextBox1.Text.Contains(" - "))
            {
                MessageBox.Show("Selecione origem e destino.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var o = txSearch.Text.Split(new[] { " - " }, StringSplitOptions.None);
            var d = guna2TextBox1.Text.Split(new[] { " - " }, StringSplitOptions.None);
            string po = o[0], co = o[1], pd = d[0], cd = d[1];

            // datas
            DateTime dataIda = DateTimePicker1.Value.Date;
            DateTime dataRetorno = guna2DateTimePicker1.Visible
                                  ? guna2DateTimePicker1.Value.Date
                                  : dataIda;

            // classe
            string classe = guna2ComboBox1.SelectedItem as string;
            if (string.IsNullOrEmpty(classe))
            {
                MessageBox.Show("Escolha a classe de assento.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // garante ClienteId
            int clienteId;
            using (var conn = new SqlConnection(connString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM Cliente WHERE UsuarioId = @u)
    SELECT Id FROM Cliente WHERE UsuarioId = @u;
ELSE
BEGIN
    INSERT INTO Cliente (Nome, Documento, UsuarioId)
    VALUES ('Nome Cliente','000000000', @u);
    SELECT SCOPE_IDENTITY();
END";
                cmd.Parameters.AddWithValue("@u", usuarioId);
                conn.Open();
                clienteId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // insere Voo
            int vooId;
            using (var conn = new SqlConnection(connString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Voo
  (PaisOrigem,CidadeOrigem,PaisDestino,CidadeDestino,DataHora,PrecoBase)
VALUES
  (@po,@co,@pd,@cd,@dh,0);
SELECT SCOPE_IDENTITY();";
                cmd.Parameters.AddWithValue("@po", po);
                cmd.Parameters.AddWithValue("@co", co);
                cmd.Parameters.AddWithValue("@pd", pd);
                cmd.Parameters.AddWithValue("@cd", cd);
                cmd.Parameters.AddWithValue("@dh", dataIda);
                conn.Open();
                vooId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // insere Reserva
            using (var conn = new SqlConnection(connString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Reserva
  (ClienteId,VooId,Classe,Assento,DataReserva,DataRetorno)
VALUES
  (@cli,@voo,@classe,@assento,GETDATE(),@retorno);";
                cmd.Parameters.AddWithValue("@cli", clienteId);
                cmd.Parameters.AddWithValue("@voo", vooId);
                cmd.Parameters.AddWithValue("@classe", classe);
                cmd.Parameters.AddWithValue("@assento", "");
                cmd.Parameters.AddWithValue("@retorno", dataRetorno);
                conn.Open();
                int linhas = cmd.ExecuteNonQuery();
                if (linhas > 0)
                    MessageBox.Show("Reserva concluída!", "Sucesso",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Falha ao criar reserva.", "Erro",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // abre recibo
            var recibo = new reciboForms();
            recibo.Show();
            this.Close();
        }
    }
}
