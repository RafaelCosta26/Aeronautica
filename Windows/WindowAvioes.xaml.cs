using Aeronautica.Models;
using Aeronautica.Models.Requests;
using Aeronautica.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Aeronautica
{
    public partial class WindowAvioes : Window
    {
        private readonly AvioeService _avioeService = new();
        private readonly LugaresService _lugaresService = new();
        private readonly MainWindow _mainWindow = new();

        public WindowAvioes(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
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

        private async void CriarAviao_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                // Verificar se os campos estão preenchidos
                if (string.IsNullOrWhiteSpace(NomeTextBox.Text) ||
                    string.IsNullOrWhiteSpace(ModeloTextBox.Text) ||
                    EstadoComboBox.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(LugaresEconomicosTextBox.Text) ||
                    string.IsNullOrWhiteSpace(LugaresExecutivosTextBox.Text))
                {
                    MessageBox.Show("Preenche todos os campos obrigatórios.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(LugaresExecutivosTextBox.Text, out int lugaresExecutivos) || lugaresExecutivos <= 0)
                {
                    MessageBox.Show("Número de lugares executivos inválido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(LugaresEconomicosTextBox.Text, out int lugaresEconomicos) || lugaresEconomicos <= 0)
                {
                    MessageBox.Show("Número de lugares económicos inválido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                BarraProgresso.Visibility = Visibility.Visible;

                var aviao = new Avioe
                {
                    Nome = NomeTextBox.Text,
                    Modelo = ModeloTextBox.Text,
                    Estado = (EstadoComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Disponível"
                };

                var response = await _avioeService.CriarAsync(aviao);

                if (response.IsSuccess)
                {
                    var response1 = await _avioeService.ObterTodosAsync();
                    var avioes = response1.Result as List<Avioe>;
                    var aviaoCriado = avioes.FirstOrDefault(a => a.Nome == aviao.Nome && a.Modelo == aviao.Modelo);
                    if (response1.IsSuccess)
                    {
                        var lugaresRequest = new GerarLugaresRequest
                        {
                            Id = aviaoCriado.IdAviao,
                            Executivos = lugaresExecutivos,
                            Economicos = lugaresEconomicos
                        };

                        var lugaresResponse = await _lugaresService.GerarLugaresParaAviaoAsync(lugaresRequest);

                        if (lugaresResponse.IsSuccess)
                        {
                            MessageBox.Show("Avião e lugares criados com sucesso!");
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Avião criado, mas erro ao gerar lugares: " + lugaresResponse.Message);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Erro ao criar avião: " + response.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Erro ao criar avião: " + response.Message);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Erro de conexão: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro inesperado: " + ex.Message);
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await _mainWindow.CarregarAvioesAsync();
        }
    }
}
