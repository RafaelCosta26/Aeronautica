using Aeronautica.Models;
using Aeronautica.Services;
using Aeronautica.ViewModels;
using Aeronautica.Windows;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aeronautica
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly VooService _vooService = new VooService();
        private readonly AeroportoService _aeroportoService = new AeroportoService();
        private readonly LugaresService _lugaresService = new LugaresService();
        private readonly AvioeService _avioeService = new AvioeService();
        private readonly PassageirosService _passageiroService = new PassageirosService();
        private readonly BilheteService _bilheteService = new BilheteService();
        private readonly LugaresVooService _lugaresVooService = new LugaresVooService();

        public MainWindow()
        {
            InitializeComponent();
            VerificarNeet();
            VerificarVoosPartidos();   
        }

        private readonly NetworkService _networkService = new NetworkService();
        private async void VerificarNeet()
        {
            var isConnected = await _networkService.CheckConnection();
            if(!isConnected.IsSuccess)
            {
                MessageBoxResult result = MessageBox.Show("Sem ligação à Internet. Deseja continuar?", "Atenção", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                   MessageBox.Show("sem internet.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void VerificarVoosPartidos()
        {
            var agora = DateTime.Now;

            var todosVoosResp = await _vooService.ObterTodosVoosAsync();
            var todosLugaresResp = await _lugaresVooService.ObterTodosAsync();
            var todosBilhetesResp = await _bilheteService.ObterTodosAsync();
            var todosPassageirosResp = await _passageiroService.ObterTodosAsync();

            if (!todosVoosResp.IsSuccess || !todosLugaresResp.IsSuccess || !todosBilhetesResp.IsSuccess || !todosPassageirosResp.IsSuccess)
                return;

            var voos = todosVoosResp.Result as List<Voo>;
            var lugares = todosLugaresResp.Result as List<LugaresVoo>;
            var bilhetes = todosBilhetesResp.Result as List<Bilhete>;
            var passageiros = todosPassageirosResp.Result as List<Passageiro>;

            if (voos == null || lugares == null || bilhetes == null || passageiros == null)
                return;

            foreach (var voo in voos)
            {
                if (DateTime.TryParse(voo.DataPartida + " " + voo.HoraPartida, out DateTime partidaDateTime) && partidaDateTime <= agora)
                {
                    var lugaresDoVoo = lugares.Where(l => l.IdVoo == voo.IdVoo).ToList();
                    var bilhetesDoVoo = bilhetes.Where(b => lugaresDoVoo.Any(l => l.IdLugarVoo == b.IdLugarVoo)).ToList();

                    foreach (var bilhete in bilhetesDoVoo)
                    {
                        var passageiro = passageiros.FirstOrDefault(p => p.IdPassageiro == bilhete.IdPassageiro);
                        if (passageiro != null)
                        {
                            if (passageiro.Nome.ToLower() == "default" && passageiro.Apelido.ToLower() == "default")
                            {
                                await _passageiroService.ApagarAsync(passageiro.IdPassageiro);
                            }
                            else
                            {
                                passageiro.VoosVoados++;
                                await _passageiroService.AtualizarAsync(passageiro.IdPassageiro, passageiro);
                            }
                        }

                        await _bilheteService.ApagarAsync(bilhete.IdBilhete);
                    }

                    foreach (var lugar in lugaresDoVoo)
                    {
                        await _lugaresVooService.ApagarAsync(lugar.IdLugarVoo);
                    }

                    await _vooService.ApagarVooAsync(voo.IdVoo);
                }
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CarregarVoosAsync();
            await CarregarAvioesAsync();
        }

        public async Task CarregarAvioesAsync()
        {
            var response = await _avioeService.ObterTodosAsync();
            if (response.IsSuccess && response.Result is List<Avioe> listaAvioes)
            {
                TabelaAvioes.ItemsSource = listaAvioes;
            }
            else
            {
                MessageBox.Show("Erro ao carregar aviões: " + response.Message);
            }
        }

        public async Task CarregarVoosAsync()
        {
            try
            {
                BarraProgresso.Visibility = Visibility.Visible;
                BarraProgresso.IsIndeterminate = true;

                var responseVoos = await _vooService.ObterTodosVoosAsync();
                var responseAeroportos = await _aeroportoService.ObterAeroportoAsync();
                var responseAvioes = await _avioeService.ObterTodosAsync();

                if (responseVoos.IsSuccess && responseVoos.Result is List<Voo> listaVoos &&
                    responseAeroportos.IsSuccess && responseAeroportos.Result is List<Aeroporto> listaAeroportos &&
                    responseAvioes.IsSuccess && responseAvioes.Result is List<Avioe> listaAvioes)
                {
                    var voosViewModel = listaVoos.Select(v => new VooViewModel
                    {
                        IdVoo = v.IdVoo,
                        AeroportoPartida = listaAeroportos.FirstOrDefault(a => a.IdAeroporto == v.IdAeroportoPartida)?.Nome ?? "Desconhecido",
                        AeroportoDestino = listaAeroportos.FirstOrDefault(a => a.IdAeroporto == v.IdAeroportoDestino)?.Nome ?? "Desconhecido",
                        Aviao = listaAvioes.FirstOrDefault(a => a.IdAviao == v.IdAviao)?.Nome ?? "Desconhecido",

                        // Formatar data para "dd/MM/yyyy"
                        DataPartida = DateTime.TryParse(v.DataPartida, out var dataPartida)
                          ? dataPartida.ToString("dd/MM/yyyy")
                          : "Data inválida",

                                          HoraPartida = v.HoraPartida,

                                          DataChegada = DateTime.TryParse(v.DataChegada, out var dataChegada)
                          ? dataChegada.ToString("dd/MM/yyyy")
                          : "Data inválida",

                        HoraChegada = v.HoraChegada,
                        RefeicaoIncluida = v.RefeicaoIncluida == "S" ? "Sim" : "Não",
                        PrecoEconomico = v.PrecoEconomico,
                        PrecoExecutivo = v.PrecoExecutivo
                    }).ToList();

                    TabelaVoos.ItemsSource = voosViewModel;
                }
                else
                {
                    MessageBox.Show("Erro ao carregar dados: " + responseVoos.Message);
                }
            }
            finally
            {
                BarraProgresso.IsIndeterminate = false;
                BarraProgresso.Visibility = Visibility.Collapsed;
            }
        }

        private void Avioes_Click(object sender, RoutedEventArgs e)
        {
            WindowAvioes avioes = new WindowAvioes(this);
            avioes.Show();
        }

        private void Aeroportos_Click(object sender, RoutedEventArgs e)
        {
            WindowAeroporto aeroportos = new WindowAeroporto();
            aeroportos.Show();
        }

        private void Voos_Click(object sender, RoutedEventArgs e)
        {
            WindowVoos voos = new WindowVoos(this);
            voos.Show();
        }

        private void Passageiros_Click(object sender, RoutedEventArgs e)
        {
            WindowPassageiros passageiros = new WindowPassageiros();
            passageiros.Show();
        }

        private void Bilhetes_Click(object sender, RoutedEventArgs e)
        {
            WindowBilhetes bilhetes = new WindowBilhetes();
            bilhetes.Show();
        }

        private void InformacaoAeroportos_Click(object sender, RoutedEventArgs e)
        {
            if (TabelaAvioes.SelectedItem is Avioe aviaoSelecionado)
            {
                var janela = new WindowInfoAviao(aviaoSelecionado, this);
                janela.Show();
            }
            else
            {
                MessageBox.Show("Seleciona um avião primeiro.");
            }
        }

        private void InformacaoVoos_Click(object sender, RoutedEventArgs e)
        {
            if(TabelaVoos.SelectedItem is VooViewModel vooSelecionado)
            {
                var janela = new WindowInfoVoo(vooSelecionado, this);
                janela.Show();
            }
            else
            {
                MessageBox.Show("Seleciona um voo primeiro.");
            }
            
        }

        private void Sobre_Click(object sender, RoutedEventArgs e)
        {
        
             MessageBox.Show("Nome: Rafael Costa\nData: 27/05/2025\nVersão: 0.2", "Sobre o Projeto", MessageBoxButton.OK, MessageBoxImage.Information);
            
        }
    }
}