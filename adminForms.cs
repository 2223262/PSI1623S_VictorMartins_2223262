using System;
using System.Windows.Forms;

namespace _DigiAirlines
{
    public partial class adminForms : Form
    {
        public adminForms()
        {
            InitializeComponent();
            this.Text = "Painel de Administração - DigiAirlines"; // Define o título da janela
        }

        private void adminForms_Load(object sender, EventArgs e)
        {
            // Pode adicionar aqui código para carregar dados que o admin precisa de ver
        }

        // Se o admin fechar este formulário, a aplicação deve fechar (ou voltar ao login)
        private void adminForms_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}