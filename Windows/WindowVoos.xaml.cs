using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using Aeronautica.Models;
using Aeronautica.Services;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Aeronautica.Windows
{
    public partial class WindowVoos : Window
    {
        private readonly VooService _vooService = new VooService();
        private readonly AeroportoService _aeroportoService = new AeroportoService();
        private readonly AvioeService _avioeService = new AvioeService();
        private readonly MainWindow _mainWindow = new();
        private List<Aeroporto> _todosAeroportos;

        public WindowVoos(MainWindow mainWindow)
        {
            InitializeComponent();
            VerificarNeet();
            Loaded += WindowVoos_Loaded;
            _mainWindow = mainWindow;
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

        private async void Window_Closing(object sender, EventArgs e)
        {
            await _mainWindow.CarregarVoosAsync();
            await _mainWindow.CarregarAvioesAsync();
        }

        private async void WindowVoos_Loaded(object sender, RoutedEventArgs e)
        {
            await CarregarComboBoxesAsync();
            DataPartidaPicker.DisplayDateStart = DateTime.Today.AddDays(1);
            DataChegadaPicker.DisplayDateStart = DateTime.Today.AddDays(1);
        }

        private async Task CarregarComboBoxesAsync()
        {
            var aeroportosResponse = await _aeroportoService.ObterAeroportoAsync();
            var avioesResponse = await _avioeService.ObterTodosAsync();

            if (aeroportosResponse.IsSuccess && aeroportosResponse.Result is List<Aeroporto> aeroportos)
            {
                _todosAeroportos = aeroportos;
                ComboAeroportoPartida.ItemsSource = aeroportos;
                ComboAeroportoDestino.ItemsSource = aeroportos;
            }

            if (avioesResponse.IsSuccess && avioesResponse.Result is List<Avioe> avioes)
            {
                // Filtra apenas os aviões com estado "Disponível"
                var avioesDisponiveis = avioes
                    .Where(a => string.Equals(a.Estado, "Disponível", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                ComboAvioes.ItemsSource = avioesDisponiveis;
            }
        }


        private void ComboAeroportoPartida_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboAeroportoPartida.SelectedItem is Aeroporto aeroportoSelecionado)
            {
                // Exclui o aeroporto de partida da lista de destinos
                ComboAeroportoDestino.ItemsSource = _todosAeroportos
                    .Where(a => a.IdAeroporto != aeroportoSelecionado.IdAeroporto)
                    .ToList();
            }
        }

        private async void CriarVoo_ClickAsync(object sender, RoutedEventArgs e)
        {
            // Verifica se todos os campos obrigatórios estão preenchidos
            if (DataPartidaPicker.SelectedDate == null)
            {
                MessageBox.Show("Seleciona uma data de partida.");
                return;
            }

            if (HoraPartidaTimePicker.Value == null)
            {
                MessageBox.Show("Seleciona uma hora de partida.");
                return;
            }

            if (DataChegadaPicker.SelectedDate == null)
            {
                MessageBox.Show("Seleciona uma data de chegada.");
                return;
            }

            if (HoraChegadaTimePicker.Value == null)
            {
                MessageBox.Show("Seleciona uma hora de chegada.");
                return;
            }

            if (ComboAeroportoPartida.SelectedItem is not Aeroporto partida)
            {
                MessageBox.Show("Seleciona um aeroporto de partida.");
                return;
            }

            if (ComboAeroportoDestino.SelectedItem is not Aeroporto destino)
            {
                MessageBox.Show("Seleciona um aeroporto de destino.");
                return;
            }

            if (partida.IdAeroporto == destino.IdAeroporto)
            {
                MessageBox.Show("O aeroporto de partida e de destino não podem ser iguais.");
                return;
            }

            if (ComboAvioes.SelectedItem is not Avioe aviao)
            {
                MessageBox.Show("Seleciona um avião.");
                return;
            }

            if (!string.Equals(aviao.Estado, "Disponível", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("O avião selecionado não está disponível.");
                return;
            }

            if (PrecoEconomicoUpDown.Value == null || PrecoEconomicoUpDown.Value <= 0)
            {
                MessageBox.Show("Indica um preço económico válido.");
                return;
            }

            if (PrecoExecutivoUpDown.Value == null || PrecoExecutivoUpDown.Value <= 0)
            {
                MessageBox.Show("Indica um preço executivo válido.");
                return;
            }

            // Construção das datas e validação temporal
            DateTime dataHoraPartida = DataPartidaPicker.SelectedDate.Value
                .Add(HoraPartidaTimePicker.Value?.TimeOfDay ?? TimeSpan.Zero);

            DateTime dataHoraChegada = DataChegadaPicker.SelectedDate.Value
                .Add(HoraChegadaTimePicker.Value?.TimeOfDay ?? TimeSpan.Zero);

            if (dataHoraChegada <= dataHoraPartida)
            {
                MessageBox.Show("A data/hora de chegada deve ser posterior à data/hora de partida.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dataHoraPartida <= DateTime.Now)
            {
                MessageBox.Show("A data/hora de partida deve ser posterior à data/hora atual.");
                return;
            }

            // Refeição
            string refeicaoIncluida = RefeicaoIncluidaCheckBox.IsChecked == true ? "S" : "N";

            // Criar o voo
            var voo = new Voo
            {
                IdAeroportoPartida = partida.IdAeroporto,
                IdAeroportoDestino = destino.IdAeroporto,
                IdAviao = aviao.IdAviao,
                DataPartida = DataPartidaPicker.SelectedDate?.ToString("yyyy-MM-dd"),
                HoraPartida = HoraPartidaTimePicker.Value?.ToString("HH:mm:ss"),
                DataChegada = DataChegadaPicker.SelectedDate?.ToString("yyyy-MM-dd"),
                HoraChegada = HoraChegadaTimePicker.Value?.ToString("HH:mm:ss"),
                RefeicaoIncluida = refeicaoIncluida,
                PrecoEconomico = PrecoEconomicoUpDown.Value ?? 0,
                PrecoExecutivo = PrecoExecutivoUpDown.Value ?? 0,
            };

            // Chamada à API
            var response = await _vooService.CriarVooAsync(voo);

            if (response.IsSuccess)
                MessageBox.Show("Voo criado com sucesso!");
            else
                MessageBox.Show("Erro ao criar voo: " + response.Message);
        }


    }
}