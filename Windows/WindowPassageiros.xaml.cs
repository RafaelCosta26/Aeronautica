using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Aeronautica.Models;
using Aeronautica.Services;

namespace Aeronautica.Windows
{
    public partial class WindowPassageiros : Window
    {
        private readonly PassageirosService _passageiroService = new PassageirosService();
        private List<Passageiro> _passageiros = new();

        public WindowPassageiros()
        {
            InitializeComponent();
            VerificarNeet();
            CarregarPassageirosAsync();
            
        }

        private readonly NetworkService _networkService = new NetworkService();
        private async void VerificarNeet()
        {
            var isConnected = await _networkService.CheckConnection();
            if (!isConnected.IsSuccess)
            {
                MessageBoxResult result = MessageBox.Show("Sem ligação à Internet. Deseja continuar?", "Atenção", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    MessageBox.Show("sem internet.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void CarregarPassageirosAsync()
        {
            BarraProgresso.Visibility = Visibility.Visible;

            var response = await _passageiroService.ObterTodosAsync();
            if (response.IsSuccess && response.Result is List<Passageiro> lista)
            {
                _passageiros = lista;
                TabelaPassageiros.ItemsSource = _passageiros;
            }
            else
            {
                MessageBox.Show("Erro ao carregar passageiros: " + response.Message);
            }

            BarraProgresso.Visibility = Visibility.Collapsed;
        }

        private async void Criar_Click(object sender, RoutedEventArgs e)
        {
            var nome = Prompt("Nome do passageiro:");
            var apelido = Prompt("Apelido:");

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(apelido))
            {
                MessageBox.Show("O nome e apelido do passageiro são obrigatórios.");
                return;
            }

            var passageiro = new Passageiro
            {
                Nome = nome,
                Apelido = apelido,
                VoosVoados = 0
            };

            BarraProgresso.Visibility = Visibility.Visible;

            var response = await _passageiroService.CriarAsync(passageiro);
            if (response.IsSuccess)
                CarregarPassageirosAsync();
            else
                MessageBox.Show("Erro: " + response.Message);

            BarraProgresso.Visibility = Visibility.Collapsed;
        }

        private async void Editar_Click(object sender, RoutedEventArgs e)
        {
            if (TabelaPassageiros.SelectedItem is not Passageiro selecionado)
            {
                MessageBox.Show("Seleciona um passageiro para editar.");
                return;
            }

            var nome = Prompt("Novo Nome:", selecionado.Nome);
            var apelido = Prompt("Novo Apelido:", selecionado.Apelido);

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(apelido))
            {
                MessageBox.Show("O nome e apelido do passageiro são obrigatórios.");
                return;
            }

            selecionado.Nome = nome;
            selecionado.Apelido = apelido;

            BarraProgresso.Visibility = Visibility.Visible;

            var response = await _passageiroService.AtualizarAsync(selecionado.IdPassageiro, selecionado);
            if (response.IsSuccess)
                CarregarPassageirosAsync();
            else
                MessageBox.Show("Erro: " + response.Message);

            BarraProgresso.Visibility = Visibility.Collapsed;
        }

        private async void Apagar_Click(object sender, RoutedEventArgs e)
        {
            if (TabelaPassageiros.SelectedItem is not Passageiro selecionado)
            {
                MessageBox.Show("Seleciona um passageiro para apagar.");
                return;
            }

            var confirm = MessageBox.Show($"Queres apagar o passageiro '{selecionado.Nome} {selecionado.Apelido}'?", "Confirmar", MessageBoxButton.YesNo);
            if (confirm != MessageBoxResult.Yes) return;

            BarraProgresso.Visibility = Visibility.Visible;

            var response = await _passageiroService.ApagarAsync(selecionado.IdPassageiro);
            if (response.IsSuccess)
                CarregarPassageirosAsync();
            else
                MessageBox.Show("Erro: " + response.Message);

            BarraProgresso.Visibility = Visibility.Collapsed;
        }

        private string? Prompt(string mensagem, string valorInicial = "")
        {
            return Microsoft.VisualBasic.Interaction.InputBox(mensagem, "Entrada de Dados", valorInicial);
        }
    }
}

