using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class menuForms : Form
    {
        private Timer relogioTimer; // Componente para atualizar o relógio
        private string connString = "Server=(localdb)\\MSSQLLocalDB;Database=DigiAirlines;Trusted_Connection=True;";
        private int clienteId = Login.ClienteLogadoId;
        private string nomeCliente = "";

        public menuForms()
        {
            InitializeComponent();
            this.Load += MenuForms_Load; // Associa o evento Load
        }

        private void MenuForms_Load(object sender, EventArgs e)
        {
            // Busca o nome do cliente na base de dados
            BuscarNomeCliente();

            // Configura a saudação
            ConfigurarSaudacao();

            // Configura e inicia o Timer para o relógio
            relogioTimer = new Timer();
            relogioTimer.Interval = 1000; // O timer dispara a cada 1000 ms (1 segundo)
            relogioTimer.Tick += RelogioTimer_Tick;
            relogioTimer.Start();

            // Atualiza a hora uma vez no início
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
                nomeCliente = "Cliente"; // Valor padrão em caso de erro
            }
        }

        private void ConfigurarSaudacao()
        {
            int horaAtual = DateTime.Now.Hour;
            string saudacao;

            if (horaAtual >= 6 && horaAtual < 12)
            {
                saudacao = "Bom dia";
            }
            else if (horaAtual >= 12 && horaAtual < 20)
            {
                saudacao = "Boa tarde";
            }
            else
            {
                saudacao = "Boa noite";
            }

            lblComprimentoUtilizador.Text = $"{saudacao}, {nomeCliente}!";
        }

        // Este evento é chamado a cada segundo pelo Timer
        private void RelogioTimer_Tick(object sender, EventArgs e)
        {
            AtualizarRelogio();
        }

        private void AtualizarRelogio()
        {
            // Atualiza o texto do label do relógio com a hora, minutos e segundos atuais
            lblRelogioReal.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        // --- EVENTOS DE CLIQUE PARA OS BOTÕES DO MENU ---

        private void guna2Button1_Click(object sender, EventArgs e) // Nova Reserva
        {
            destinoForms formReservas = new destinoForms();
            formReservas.ShowDialog(); // ShowDialog foca no formulário de reserva
        }

        private void guna2Button2_Click(object sender, EventArgs e) // Minhas Reservas
        {
            // Nota: Certifique-se de que o seu formulário de histórico se chama 'HistoricoClienteForm'
            // ou altere o nome da classe abaixo para corresponder ao seu.
            minhasReservasForms formHistorico = new minhasReservasForms();
            formHistorico.ShowDialog();
        }

        private void guna2Button3_Click(object sender, EventArgs e) // Meu Perfil
        {
            MessageBox.Show("Funcionalidade para gerir o perfil ainda não implementada.", "Em Construção", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void guna2Button4_Click(object sender, EventArgs e) // Sair
        {
            // Pede confirmação antes de fechar a aplicação
            var confirmResult = MessageBox.Show("Tem a certeza que deseja fechar a aplicação?", "Confirmar Saída", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }


        // Garante que a aplicação fecha se o utilizador fechar esta janela pelo "X"
        private void menuForms_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (relogioTimer != null)
            {
                relogioTimer.Stop();
                relogioTimer.Dispose();
            }
            Application.Exit();
        }

        // Os métodos abaixo podem ser deixados em branco, pois os labels não precisam de ação ao serem clicados
        private void lblComprimentoUtilizador_Click(object sender, EventArgs e)
        {

        }

        private void lblRelogioReal_Click(object sender, EventArgs e)
        {

        }
    }
}