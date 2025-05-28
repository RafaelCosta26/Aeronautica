using Aeronautica.Models;
using Aeronautica.Services;
using System.Windows;
using System.Windows.Controls;

namespace Aeronautica.Windows
{
    public partial class WindowInfoAviao : Window
    {
        private readonly AvioeService _aviaoService = new();
        private readonly LugaresService _lugaresService = new();
        private readonly MainWindow _mainWindow;
        private readonly VooService _vooService = new();
        private Avioe _aviao;

        public WindowInfoAviao(Avioe aviao, MainWindow mainWindow)
        {
            InitializeComponent();
            VerificarNeet();
            _mainWindow = mainWindow;
            _aviao = aviao;
            NomeTextBox.Text = aviao.Nome;
            ModeloTextBox.Text = aviao.Modelo;
            EstadoComboBox.SelectedIndex = aviao.Estado == "Disponível" ? 0 : 1;
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

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            BarraProgresso.Visibility = Visibility.Visible;

            // Obter o novo estado selecionado
            string novoEstado = (EstadoComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Disponível";

            // Verificar se está a tentar mudar o estado
            if (_aviao.Estado != novoEstado)
            {
                // Obter todos os voos
                var voosResponse = await _vooService.ObterTodosVoosAsync();
                if (voosResponse.IsSuccess && voosResponse.Result is List<Voo> todosOsVoos)
                {
                    foreach (var voo in todosOsVoos)
                    {
                        if (voo.IdAviao == _aviao.IdAviao)
                        {
                            MessageBox.Show("Este avião já tem voos associados. Não é possível alterar o estado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                            BarraProgresso.Visibility = Visibility.Collapsed;
                            return;
                        }
                    }
                }
            }

            // Atualizar os dados
            _aviao.Nome = NomeTextBox.Text;
            _aviao.Modelo = ModeloTextBox.Text;
            _aviao.Estado = novoEstado;

            var response = await _aviaoService.AtualizarAsync(_aviao.IdAviao, _aviao);

            BarraProgresso.Visibility = Visibility.Collapsed;

            if (response.IsSuccess)
            {
                MessageBox.Show("Avião atualizado com sucesso.");
                Close();
            }
            else
            {
                MessageBox.Show("Erro: " + response.Message);
            }
        }

        private async void Apagar_Click(object sender, RoutedEventArgs e)
        {
            var confirmar = MessageBox.Show("Tem a certeza que deseja apagar este avião?", "Confirmar", MessageBoxButton.YesNo);
            if (confirmar != MessageBoxResult.Yes) return;

            BarraProgresso.Visibility = Visibility.Visible;

            var response1 = await _lugaresService.ApagarAsync(_aviao.IdAviao);
            var response = await _aviaoService.ApagarAsync(_aviao.IdAviao);

            BarraProgresso.Visibility = Visibility.Collapsed;

            if (response.IsSuccess)
            {
                MessageBox.Show("Avião apagado.");
                Close();
            }
            else
            {
                MessageBox.Show("Erro: " + response.Message);
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           await _mainWindow.CarregarAvioesAsync();
        }
    }
}
