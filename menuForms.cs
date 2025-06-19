using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class menuForms : Form
    {
        private Timer relogioTimer;
        private string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        private int clienteId = Login.ClienteLogadoId;
        private string nomeCliente = "";

        public menuForms()
        {
            InitializeComponent();
            this.Load += MenuForms_Load;
        }

        private void MenuForms_Load(object sender, EventArgs e)
        {
            BuscarNomeCliente();
            ConfigurarSaudacao();

            relogioTimer = new Timer();
            relogioTimer.Interval = 1000;
            relogioTimer.Tick += RelogioTimer_Tick;
            relogioTimer.Start();

            AtualizarRelogio();
        }

        private void BuscarNomeCliente()
        {
            if (clienteId <= 0) return;

            string query = "SELECT Nome FROM Cliente WHERE Id = @clienteId";
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        nomeCliente = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possível obter os dados do utilizador: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                nomeCliente = "Cliente";
            }
        }

        private void ConfigurarSaudacao()
        {
            int horaAtual = DateTime.Now.Hour;
            string saudacao = "Olá";

            if (horaAtual >= 6 && horaAtual < 12) { saudacao = "Bom dia"; }
            else if (horaAtual >= 12 && horaAtual < 20) { saudacao = "Boa tarde"; }
            else { saudacao = "Boa noite"; }

            lblComprimentoUtilizador.Text = $"{saudacao} {nomeCliente}☺";
        }

        private void RelogioTimer_Tick(object sender, EventArgs e)
        {
            AtualizarRelogio();
        }

        private void AtualizarRelogio()
        {
            lblRelogioReal.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        // --- EVENTOS DE CLIQUE PARA OS BOTÕES DO MENU (ATUALIZADOS) ---

        private void guna2Button1_Click(object sender, EventArgs e) // Nova Reserva
        {

        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            destinoForms formReservas = new destinoForms();
            this.Hide(); // Esconde o menu atual
            formReservas.FormClosed += (s, args) => this.Show();
            formReservas.ShowDialog();
        }

        private void guna2Button2_Click(object sender, EventArgs e) // Minhas Reservas
        {
            // Esconde o menu, mostra o histórico, e quando o histórico for fechado, o menu volta a aparecer.
            this.Hide();

            // Nota: Se o seu formulário de histórico se chamar 'minhasReservasForms', altere o nome da classe abaixo.
            minhasReservasForms formHistorico = new minhasReservasForms();
            formHistorico.FormClosed += (s, args) => this.Show(); // Adiciona um evento para mostrar o menu quando o histórico fechar
            formHistorico.Show();
        }

        private void guna2Button3_Click(object sender, EventArgs e) // Meu Perfil
        {
            this.Hide();
            // Abre o novo formulário para o perfil do utilizador.
            meuPerfilForms formPerfil = new meuPerfilForms();
            formPerfil.FormClosed += (s, args) => this.Show(); // Adiciona um evento para mostrar o menu quando o perfil fechar
            formPerfil.ShowDialog();
        }

        private void guna2Button4_Click(object sender, EventArgs e) // Sair
        {
            var confirmResult = MessageBox.Show("Tem a certeza que deseja fechar a aplicação?", "Confirmar Saída", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void menuForms_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (relogioTimer != null)
            {
                relogioTimer.Stop();
                relogioTimer.Dispose();
            }
            Application.Exit();
        }

        private void lblComprimentoUtilizador_Click(object sender, EventArgs e) { }
        private void lblRelogioReal_Click(object sender, EventArgs e) { }

    }
}