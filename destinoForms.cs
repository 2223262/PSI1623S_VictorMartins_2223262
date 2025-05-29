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

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            reciboForms forms4 = new reciboForms();
            // 1) Extrai origem e destino dos dois textboxes:
            //    txSearch     → "PaísOrigem - CidadeOrigem"
            //    guna2TextBox1→ "PaísDestino - CidadeDestino"
            if (!txSearch.Text.Contains(" - ") || !guna2TextBox1.Text.Contains(" - "))
            {
                MessageBox.Show("Selecione primeiro origem e destino nas grids.",
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var partsOrigem = txSearch.Text.Split(new[] { " - " }, StringSplitOptions.None);
            var partsDestino = guna2TextBox1.Text.Split(new[] { " - " }, StringSplitOptions.None);

            string paisOrig = partsOrigem[0].Trim();
            string cidadeOrig = partsOrigem[1].Trim();
            string paisDest = partsDestino[0].Trim();
            string cidadeDest = partsDestino[1].Trim();

            // 2) Datas
            DateTime dataIda = DateTimePicker1.Value;
            DateTime dataRetorno = guna2DateTimePicker1.Value;

            // 3) Classe de assento
            var classe = guna2ComboBox1.SelectedItem as string;
            if (string.IsNullOrEmpty(classe))
            {
                MessageBox.Show("Escolha o tipo de assento.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4) Preço base: vamos buscar da tabela Destino (origem→destino)
            decimal precoBase = 0;
            using (var connPre = new SqlConnection(connString))
            using (var cmdPre = connPre.CreateCommand())
            {
                connPre.Open();
                cmdPre.CommandText = @"
          SELECT Preco 
            FROM Destino 
           WHERE Pais = @po  AND Cidade = @co
             AND Pais = @pd  AND Cidade = @cd";
                cmdPre.Parameters.AddWithValue("@po", paisOrig);
                cmdPre.Parameters.AddWithValue("@co", cidadeOrig);
                cmdPre.Parameters.AddWithValue("@pd", paisDest);
                cmdPre.Parameters.AddWithValue("@cd", cidadeDest);

                var obj = cmdPre.ExecuteScalar();
                precoBase = (obj == null ? 0 : Convert.ToDecimal(obj));
            }

            // 5) Insere o voo
            int vooId;
            using (var conn = new SqlConnection(connString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
          INSERT INTO Voo
            (PaisOrigem, CidadeOrigem, PaisDestino, CidadeDestino, DataHora, PrecoBase)
          VALUES
            (@po, @co, @pd, @cd, @dh, @preco);
          SELECT SCOPE_IDENTITY();
        ";
                cmd.Parameters.AddWithValue("@po", paisOrig);
                cmd.Parameters.AddWithValue("@co", cidadeOrig);
                cmd.Parameters.AddWithValue("@pd", paisDest);
                cmd.Parameters.AddWithValue("@cd", cidadeDest);
                cmd.Parameters.AddWithValue("@dh", dataIda);
                cmd.Parameters.AddWithValue("@preco", precoBase);

                vooId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // 6) Recupera o ClienteId (por ex. do login; aqui uso o PerfilDefault = 1)
            int clienteId = 1; // ou carregue de onde você guardou após o login

            // 7) Insere Reserva (incluindo data de retorno)
            using (var connR = new SqlConnection(connString))
            using (var cmdR = connR.CreateCommand())
            {
                connR.Open();
                cmdR.CommandText = @"
          INSERT INTO Reserva
            (ClienteId, VooId, Classe, Assento, DataReserva, DataRetorno)
          VALUES
            (@cli, @voo, @classe, @assento, GETDATE(), @retorno);
        ";
                cmdR.Parameters.AddWithValue("@cli", clienteId);
                cmdR.Parameters.AddWithValue("@voo", vooId);
                cmdR.Parameters.AddWithValue("@classe", classe);
                cmdR.Parameters.AddWithValue("@assento", ""); // você não tem txtAssento: poderia usar uma combo ou gerar automático
                cmdR.Parameters.AddWithValue("@retorno", dataRetorno);

                int linhas = cmdR.ExecuteNonQuery();
                if (linhas > 0)
                    MessageBox.Show("Reserva concluída!", "Sucesso",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Falha ao reservar.", "Erro",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
