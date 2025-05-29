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
using MySql.Data.MySqlClient;
using Mysqlx.Connection;

namespace _DigiAirlines
{
    public partial class destinoForms : Form
    {
        string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        private void loadResults()
        {
            SqlConnection conn = new SqlConnection(connString);
            conn.Open();

            // 3. Comando SQL
            string sql = "SELECT Pais, Cidade FROM Destino";

            // 4. Criar comando
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            // 5. Preencher DataTable com SqlDataAdapter
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);

            // 6. Enviar para DataGridView (ou outro controle)
            searchResult.DataSource = dt;

            // 7. Limpar recursos
            da.Dispose();
            cmd.Dispose();
            conn.Close();

        }
        public destinoForms()
        {
            InitializeComponent();
            label4.Visible = false;
            guna2ComboBox1.Visible = false;
            guna2DateTimePicker1.Visible = false;
            label5.Visible = false;

        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void searchResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void destinoForms_Load(object sender, EventArgs e)
        {
            /*loadResults();*/
        }

        private void txSearch_TextChanged(object sender, EventArgs e)
        {
            // Se tiver menos de 2 caracteres, esconde o grid e sai
            if (txSearch.TextLength < 2)
            {
                searchResult.Visible = false;
                searchResult.DataSource = null;
                return;
            }

            // Caso tenha 2+ caracteres, faz a busca
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

            // Só mostra o grid se trouxe resultados
            searchResult.Visible = (searchResult.Rows.Count > 0);
        }



        private void searchResult_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;          // evita cabeçalho
            var row = searchResult.Rows[e.RowIndex];

            // Coluna 0 = Pais, Coluna 1 = Cidade
            string pais = row.Cells[0].Value?.ToString();
            string cidade = row.Cells[1].Value?.ToString();

            // Preenche o txSearch com "País - Cidade"
            txSearch.Text = $"{pais} - {cidade}";

            // Opcional: esconde o grid para parecer um dropdown fechado
            searchResult.Visible = false;
            
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox1_Click(object sender, EventArgs e)
        {
            bool marcado = guna2CustomCheckBox1.Checked;
            guna2DateTimePicker1.Visible = marcado;
            label5.Visible = marcado;
        }

        private int usuarioId = 1; // Exemplo: deve vir do seu fluxo de login

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // 1) Valida seleções de origem/destino
            if (!txSearch.Text.Contains(" - ") || !guna2TextBox1.Text.Contains(" - "))
            {
                MessageBox.Show("Selecione Origem e Destino.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var partsO = txSearch.Text.Split(new[] { " - " }, StringSplitOptions.None);
            var partsD = guna2TextBox1.Text.Split(new[] { " - " }, StringSplitOptions.None);
            string po = partsO[0].Trim(), co = partsO[1].Trim();
            string pd = partsD[0].Trim(), cd = partsD[1].Trim();

            // 2) Datas
            DateTime dataIda = DateTimePicker1.Value.Date;
            DateTime dataRetorno = guna2DateTimePicker1.Visible
                                    ? guna2DateTimePicker1.Value.Date
                                    : (DateTime?)null ?? dataIda;

            // 3) Classe de assento
            string classe = guna2ComboBox1.SelectedItem as string;
            if (string.IsNullOrEmpty(classe))
            {
                MessageBox.Show("Escolha a classe de assento.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4) Garante ClienteId (insere se necessário)
            int clienteId;
            using (var conn = new SqlConnection(connString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
IF EXISTS(SELECT 1 FROM Cliente WHERE UsuarioId = @u)
    SELECT Id FROM Cliente WHERE UsuarioId = @u;
ELSE
BEGIN
    INSERT INTO Cliente (Nome, Documento, UsuarioId)
    VALUES (@nome, @doc, @u);
    SELECT SCOPE_IDENTITY();
END";
                cmd.Parameters.AddWithValue("@u", usuarioId);
                // estes campos você precisaria coletar: nome e documento do cliente
                cmd.Parameters.AddWithValue("@nome", "Nome do Cliente");
                cmd.Parameters.AddWithValue("@doc", "000000000");
                conn.Open();
                clienteId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // 5) Insere o Voo e obtém o VooId
            int vooId;
            using (var conn = new SqlConnection(connString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Voo
  (PaisOrigem, CidadeOrigem, PaisDestino, CidadeDestino, DataHora, PrecoBase)
VALUES
  (@po, @co, @pd, @cd, @dh, 0);
SELECT SCOPE_IDENTITY();";
                cmd.Parameters.AddWithValue("@po", po);
                cmd.Parameters.AddWithValue("@co", co);
                cmd.Parameters.AddWithValue("@pd", pd);
                cmd.Parameters.AddWithValue("@cd", cd);
                cmd.Parameters.AddWithValue("@dh", dataIda);
                conn.Open();
                vooId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // 6) Insere a Reserva
            using (var conn = new SqlConnection(connString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Reserva
  (ClienteId, VooId, Classe, Assento, DataReserva, DataRetorno)
VALUES
  (@cli, @voo, @classe, @assento, GETDATE(), @retorno);";
                cmd.Parameters.AddWithValue("@cli", clienteId);
                cmd.Parameters.AddWithValue("@voo", vooId);
                cmd.Parameters.AddWithValue("@classe", classe);
                cmd.Parameters.AddWithValue("@assento", "");  // se tiver controle de assento, substitua aqui
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

            // 7) Abre recibo
            var recibo = new reciboForms();
            recibo.Show();
            this.Close();
        }


        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (DateTimePicker1.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Data inválida! Apenas datas a partir de hoje são permitidas.",
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Reseta para hoje
                DateTimePicker1.Value = DateTime.Today;
            }
        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // Se a data selecionada for anterior a hoje
            if (guna2DateTimePicker1.Value.Date < DateTime.Today)
            {
                MessageBox.Show(
                    "Data inválida! Apenas datas a partir de hoje são permitidas.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                // Reseta para hoje
                guna2DateTimePicker1.Value = DateTime.Today;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;          // evita cabeçalho
            var row = dataGridView1.Rows[e.RowIndex];

            // Coluna 0 = Pais, Coluna 1 = Cidade
            string pais = row.Cells[0].Value?.ToString();
            string cidade = row.Cells[1].Value?.ToString();

            // Preenche o txSearch com "País - Cidade"
            guna2TextBox1.Text = $"{pais} - {cidade}";

            // Opcional: esconde o grid para parecer um dropdown fechado
            dataGridView1.Visible = false;
            label4.Visible = true;
            guna2ComboBox1.Visible = true;
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
                // **Um** parâmetro @pais
                cmd.Parameters.AddWithValue("@pais", guna2TextBox1.Text + "%");
                // **Um** parâmetro @cidade
                cmd.Parameters.AddWithValue("@cidade", guna2TextBox1.Text + "%");

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                dataGridView1.DataSource = dt;
            }

            dataGridView1.Visible = (dataGridView1.Rows.Count > 0);
        }


    }
}
