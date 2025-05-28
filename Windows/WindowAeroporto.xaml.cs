using Aeronautica.Models;
using Aeronautica.Services;
using System.Collections.Generic;
using System.Windows;

namespace Aeronautica.Windows
{
    public partial class WindowAeroporto : Window
    {
        private readonly AeroportoService _aeroportoService = new AeroportoService();
        private List<Aeroporto> _aeroportos = new();

        public WindowAeroporto()
        {
            InitializeComponent();
            CarregarAeroportosAsync();
            VerificarNeet();
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

        private async void CarregarAeroportosAsync()
        {
            BarraProgresso.Visibility = Visibility.Visible;

            var response = await _aeroportoService.ObterAeroportoAsync();
            if (response.IsSuccess && response.Result is List<Aeroporto> lista)
            {
                _aeroportos = lista;
                TabelaAeroportos.ItemsSource = _aeroportos;
            }
            else
            {
                MessageBox.Show("Erro ao carregar aeroportos: " + response.Message);
            }

            BarraProgresso.Visibility = Visibility.Collapsed;
        }

        private async void Criar_Click(object sender, RoutedEventArgs e)
        {
            var nome = Prompt("Nome do Aeroporto:");
            var codigo = Prompt("Código (ex: LIS):");
            var pais = Prompt("País:");
            var cidade = Prompt("Cidade:");

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(codigo) ||
                string.IsNullOrWhiteSpace(pais) || string.IsNullOrWhiteSpace(cidade))
            {
                MessageBox.Show("Todos os campos devem ser preenchidos.");
                return;
            }

            if (codigo.Length > 3)
            {
                MessageBox.Show("O código deve ter no máximo 3 caracteres.");
                return;
            }

            var aeroporto = new Aeroporto
            {
                Nome = nome,
                CodigoLIS = codigo,
                Pais = pais,
                Cidade = cidade
            };

            BarraProgresso.Visibility = Visibility.Visible;

            var response = await _aeroportoService.CriarAeroportoAsync(aeroporto);
            if (response.IsSuccess)
                CarregarAeroportosAsync();
            else
                MessageBox.Show("Erro: " + response.Message);

            BarraProgresso.Visibility = Visibility.Collapsed;
        }

        private async void Editar_Click(object sender, RoutedEventArgs e)
        {
            if (TabelaAeroportos.SelectedItem is not Aeroporto selecionado)
            {
                MessageBox.Show("Seleciona um aeroporto para editar.");
                return;
            }

            var nome = Prompt("Novo Nome:", selecionado.Nome);
            var codigo = Prompt("Novo Código:", selecionado.CodigoLIS);
            var pais = Prompt("Novo País:", selecionado.Pais);
            var cidade = Prompt("Nova Cidade:", selecionado.Cidade);

            if (nome == null || codigo == null || pais == null || cidade == null)
                return;

            selecionado.Nome = nome;
            selecionado.CodigoLIS = codigo;
            selecionado.Pais = pais;
            selecionado.Cidade = cidade;

            BarraProgresso.Visibility = Visibility.Visible;

            var response = await _aeroportoService.AtualizAraeroportoAsync(selecionado.IdAeroporto, selecionado);
            if (response.IsSuccess)
                CarregarAeroportosAsync();
            else
                MessageBox.Show("Erro: " + response.Message);

            BarraProgresso.Visibility = Visibility.Collapsed;
        }

        private async void Apagar_Click(object sender, RoutedEventArgs e)
        {
            if (TabelaAeroportos.SelectedItem is not Aeroporto selecionado)
            {
                MessageBox.Show("Seleciona um aeroporto para apagar.");
                return;
            }

            var confirm = MessageBox.Show($"Queres apagar o aeroporto '{selecionado.Nome}'?", "Confirmar", MessageBoxButton.YesNo);
            if (confirm != MessageBoxResult.Yes) return;

            BarraProgresso.Visibility = Visibility.Visible;

            var response = await _aeroportoService.ApagarAeroportoAsync(selecionado.IdAeroporto);
            if (response.IsSuccess)
                CarregarAeroportosAsync();
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
