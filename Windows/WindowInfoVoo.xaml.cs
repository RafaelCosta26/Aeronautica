using Aeronautica.Models;
using Aeronautica.Services;
using Aeronautica.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Aeronautica.Windows
{
    public partial class WindowInfoVoo : Window
    {
        private readonly VooService _vooService = new();
        private readonly AvioeService _avioeService = new();
        private VooViewModel _vooViewModel; // Passed in, contains display data
        private Voo _vooCompleto;      // Loaded from API, source of truth for editing
        private MainWindow _mainWuindow;

        private List<Avioe> _listaAvioes = new(); // For ComboBox ItemsSource

        // The _lugares field was uninitialized and its logic in Guardar_Click was flawed.
        // Server-side validation for seat changes is more reliable.
        // private List<Lugar> _lugares; 

        public WindowInfoVoo(VooViewModel vooViewModel, MainWindow mainWindow)
        {
            InitializeComponent();
            VerificarNeet();
            _vooViewModel = vooViewModel;
            _mainWuindow = mainWindow;
            // Avoid async calls or complex UI setup in the constructor.
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BarraProgresso.Visibility = Visibility.Visible;
            BarraProgresso.IsIndeterminate = true;
            bool loadSuccess = true;

            // 1. Load the full Voo object (_vooCompleto) for editing
            var vooResponse = await _vooService.ObterVooPorIdAsync(_vooViewModel.IdVoo);
            if (vooResponse.IsSuccess && vooResponse.Result is Voo fullVoo)
            {
                _vooCompleto = fullVoo;
            }
            else
            {
                MessageBox.Show($"Erro ao carregar detalhes do voo: {vooResponse.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                loadSuccess = false;
            }

            // 2. Load airplanes for the ComboBox
            if (loadSuccess)
            {
                var avioesResponse = await _avioeService.ObterTodosAsync();
                if (avioesResponse.IsSuccess && avioesResponse.Result is List<Avioe> avioes)
                {
                    _listaAvioes = avioes
                         .Where(a => string.Equals(a.Estado, "Disponível", StringComparison.OrdinalIgnoreCase))
                         .ToList();
                    ComboAviao.ItemsSource = _listaAvioes;
                    ComboAviao.DisplayMemberPath = "Nome";
                    ComboAviao.SelectedValuePath = "IdAviao"; // Expects int
                }
                else
                {
                    MessageBox.Show($"Erro ao carregar lista de aviões: {avioesResponse.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    loadSuccess = false;
                }
            }

            // 3. Populate UI fields if all data loaded successfully
            if (loadSuccess && _vooCompleto != null)
            {
                // Set ComboBox selected value using the ID from the fully loaded Voo object
                ComboAviao.SelectedValue = _vooCompleto.IdAviao;

                // Populate other fields from _vooCompleto
                TextDataPartida.Value = DateTime.TryParse(_vooCompleto.DataPartida, out var dataPartida) ? dataPartida : (DateTime?)null;
                TextHoraPartida.Text = _vooCompleto.HoraPartida;
                TextDataChegada.Value = DateTime.TryParse(_vooCompleto.DataChegada, out var dataChegada) ? dataChegada : (DateTime?)null;
                TextHoraChegada.Text = _vooCompleto.HoraChegada;
                CheckRefeicao.IsChecked = _vooCompleto.RefeicaoIncluida == "S";
                TextPrecoEcon.Value = _vooCompleto.PrecoEconomico;
                TextPrecoExec.Value = _vooCompleto.PrecoExecutivo;
            }
            else
            {
                // If loading failed, potentially close the window or disable controls
                MessageBox.Show("Não foi possível carregar todos os dados necessários. A janela será fechada.", "Erro de Carregamento", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
            }

            BarraProgresso.IsIndeterminate = false;
            BarraProgresso.Visibility = Visibility.Collapsed;
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (_vooCompleto == null)
            {
                MessageBox.Show("Os dados do voo não foram carregados corretamente. Não é possível guardar.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ComboAviao.SelectedItem is not Avioe novoAviaoSelecionado)
            {
                MessageBox.Show("Por favor, selecione um avião.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.Equals(novoAviaoSelecionado.Estado, "Disponível", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("O avião selecionado não está disponível. Por favor, escolha outro.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Basic validation for dates (more can be added)
            if (TextDataPartida.Value == null || TextDataChegada.Value == null)
            {
                MessageBox.Show("As datas de partida e chegada são obrigatórias.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (TimeSpan.TryParse(TextHoraPartida.Text, out _) == false || TimeSpan.TryParse(TextHoraChegada.Text, out _) == false)
            {
                MessageBox.Show("Formato de hora inválido. Use HH:mm ou HH:mm:ss.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Add logical date/time validation (e.g., arrival after departure)

            // Update the _vooCompleto object with values from the UI
            _vooCompleto.IdAviao = novoAviaoSelecionado.IdAviao;
            _vooCompleto.DataPartida = TextDataPartida.Value?.ToString("yyyy-MM-dd");
            _vooCompleto.HoraPartida = TextHoraPartida.Text; // Assuming format HH:mm or HH:mm:ss
            _vooCompleto.DataChegada = TextDataChegada.Value?.ToString("yyyy-MM-dd");
            _vooCompleto.HoraChegada = TextHoraChegada.Text; // Assuming format HH:mm or HH:mm:ss
            _vooCompleto.RefeicaoIncluida = CheckRefeicao.IsChecked == true ? "S" : "N";
            _vooCompleto.PrecoEconomico = TextPrecoEcon.Value ?? 0;
            _vooCompleto.PrecoExecutivo = TextPrecoExec.Value ?? 0;

            BarraProgresso.Visibility = Visibility.Visible;
            BarraProgresso.IsIndeterminate = true;

            var response = await _vooService.AtualizarVooAsync(_vooCompleto.IdVoo, _vooCompleto);

            BarraProgresso.IsIndeterminate = false;
            BarraProgresso.Visibility = Visibility.Collapsed;

            if (response.IsSuccess)
            {
                MessageBox.Show("Voo atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                // Optionally, set DialogResult if this window is used as a dialog
                // DialogResult = true; 
                Close();
            }
            else
            {
                MessageBox.Show($"Erro ao atualizar o voo: {response.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainWuindow.CarregarVoosAsync(); 
        }

        private async void Apagar_Click(object sender, RoutedEventArgs e)
        {
            // Ensure the flight data is loaded
            if (_vooCompleto == null)
            {
                MessageBox.Show("Os dados do voo não foram carregados corretamente. Não é possível apagar.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Confirm with the user
            MessageBoxResult confirmacao = MessageBox.Show(
                $"Tem a certeza que deseja apagar o voo ID: {_vooCompleto.IdVoo} ({_vooViewModel?.AeroportoPartida} para {_vooViewModel?.AeroportoDestino})?", // Added more details from ViewModel for clarity
                "Confirmar Apagamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacao == MessageBoxResult.Yes)
            {
                BarraProgresso.Visibility = Visibility.Visible;
                // If your ProgressBar is not set to IsIndeterminate="True" in XAML by default when visible:
                // BarraProgresso.IsIndeterminate = true; 

                // Call the service to delete the flight
                var response = await _vooService.ApagarVooAsync(_vooCompleto.IdVoo);

                // If your ProgressBar was set to IsIndeterminate="true" above:
                // BarraProgresso.IsIndeterminate = false;
                BarraProgresso.Visibility = Visibility.Collapsed;

                if (response.IsSuccess)
                {
                    MessageBox.Show("Voo apagado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close(); // Close the window after successful deletion
                }
                else
                {
                    MessageBox.Show($"Erro ao apagar o voo: {response.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}