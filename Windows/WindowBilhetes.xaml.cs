using Aeronautica.Models;
using Aeronautica.Services;
using Aeronautica.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Aeronautica.Windows
{
    public partial class WindowBilhetes : Window
    {
        private readonly BilheteService _bilheteService = new();
        private readonly VooService _vooService = new();
        private readonly PassageirosService _passageiroService = new();
        private readonly LugaresVooService _lugaresVooService = new();
        private readonly AeroportoService _aeroportoService = new();

        private List<Bilhete> _bilhetes = new();
        private List<Voo> _voos = new();
        private List<Aeroporto> _aeroportos = new();
        private List<Passageiro> _passageiros = new();
        private List<LugaresVoo> _lugaresVoo = new();
        private List<VooViewModel> _vooViewModels = new();

        private Bilhete _bilheteSelecionado; // usado na edição

        public WindowBilhetes()
        {
            InitializeComponent();
            CarregarDadosAsync();
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

        private async void CarregarDadosAsync()
        {
            BarraProgresso.Visibility = Visibility.Visible;
            BarraProgresso.IsIndeterminate = true;

            var bilheteTask = _bilheteService.ObterTodosAsync();
            var vooTask = _vooService.ObterTodosVoosAsync();
            var passageiroTask = _passageiroService.ObterTodosAsync();
            var lugarTask = _lugaresVooService.ObterTodosAsync();
            var aeroportoTask = _aeroportoService.ObterAeroportoAsync();

            await Task.WhenAll(bilheteTask, vooTask, passageiroTask, lugarTask, aeroportoTask);

            var bilheteResp = await bilheteTask;
            var vooResp = await vooTask;
            var passageiroResp = await passageiroTask;
            var lugarResp = await lugarTask;
            var aeroportoResp = await aeroportoTask;

            if (bilheteResp.IsSuccess && bilheteResp.Result is List<Bilhete> bilhetes)
                _bilhetes = bilhetes;

            if (aeroportoResp.IsSuccess && aeroportoResp.Result is List<Aeroporto> aeroportos)
                _aeroportos = aeroportos;

            if (vooResp.IsSuccess && vooResp.Result is List<Voo> voos)
            {
                _voos = voos;

                _vooViewModels = voos.Select(v => new VooViewModel
                {
                    IdVoo = v.IdVoo,
                    AeroportoPartida = _aeroportos.FirstOrDefault(a => a.IdAeroporto == v.IdAeroportoPartida)?.Nome ?? "Desconhecido",
                    AeroportoDestino = _aeroportos.FirstOrDefault(a => a.IdAeroporto == v.IdAeroportoDestino)?.Nome ?? "Desconhecido",
                    Aviao = v.IdAviao.ToString(),
                    DataPartida = v.DataPartida,
                    HoraPartida = v.HoraPartida,
                    DataChegada = v.DataChegada,
                    HoraChegada = v.HoraChegada,
                    RefeicaoIncluida = v.RefeicaoIncluida,
                    PrecoEconomico = v.PrecoEconomico,
                    PrecoExecutivo = v.PrecoExecutivo
                }).ToList();
            }

            if (passageiroResp.IsSuccess && passageiroResp.Result is List<Passageiro> passageiros)
                _passageiros = passageiros;

            if (lugarResp.IsSuccess && lugarResp.Result is List<LugaresVoo> lugares)
                _lugaresVoo = lugares;

            var bilhetesCompletos = _bilhetes.Select(b =>
            {
                var passageiro = _passageiros.FirstOrDefault(p => p.IdPassageiro == b.IdPassageiro);
                var lugar = _lugaresVoo.FirstOrDefault(l => l.IdLugarVoo == b.IdLugarVoo);

                return new BilheteViewModel
                {
                    IdBilhete = b.IdBilhete,
                    NomePassageiro = passageiro != null ? $"{passageiro.Nome} {passageiro.Apelido}" : "Desconhecido",
                    LugarDescricao = lugar != null ? $"{lugar.Fila}{lugar.Coluna} ({lugar.Tipo})" : "Desconhecido",
                    Preco = (int)b.Preco
                };
            }).ToList();

            TabelaBilhetes.ItemsSource = bilhetesCompletos;

            ComboVoo.ItemsSource = _vooViewModels;
            ComboVoo.DisplayMemberPath = "Nome";
            ComboVoo.SelectedValuePath = "IdVoo";

            ComboPassageiro.ItemsSource = _passageiros;
            ComboPassageiro.DisplayMemberPath = "NomeCompleto";
            ComboPassageiro.SelectedValuePath = "IdPassageiro";

            BarraProgresso.IsIndeterminate = false;
            BarraProgresso.Visibility = Visibility.Collapsed;
        }

        private void ComboVoo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboLugar.ItemsSource = null;
            if (ComboVoo.SelectedItem is VooViewModel vooSelecionado)
            {
                var lugaresDisponiveis = _lugaresVoo
                    .Where(l => l.IdVoo == vooSelecionado.IdVoo && l.Estado.Equals("Disponível", StringComparison.OrdinalIgnoreCase))
                    .Select(l => new LugarVooViewModel
                    {
                        IdLugarVoo = l.IdLugarVoo,
                        Descricao = $"{l.Fila}{l.Coluna} ({l.Tipo})",
                        Tipo = l.Tipo
                    })
                    .OrderBy(l => l.Tipo)
                    .ThenBy(l => l.Descricao)
                    .ToList();

                ComboLugar.ItemsSource = lugaresDisponiveis;
                ComboLugar.DisplayMemberPath = "Descricao";
            }
        }

        private void Editar_Click(object sender, RoutedEventArgs e)
        {
            if (TabelaBilhetes.SelectedItem is not BilheteViewModel bilheteVM)
            {
                MessageBox.Show("Seleciona um bilhete da lista para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _bilheteSelecionado = _bilhetes.FirstOrDefault(b => b.IdBilhete == bilheteVM.IdBilhete);
            if (_bilheteSelecionado == null)
            {
                MessageBox.Show("Bilhete não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ComboPassageiro.SelectedItem = _passageiros.FirstOrDefault(p => p.IdPassageiro == _bilheteSelecionado.IdPassageiro);
            var voo = _vooViewModels.FirstOrDefault(v => _lugaresVoo.Any(l => l.IdLugarVoo == _bilheteSelecionado.IdLugarVoo && l.IdVoo == v.IdVoo));
            ComboVoo.SelectedItem = voo;
            ComboVoo_SelectionChanged(ComboVoo, new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new List<object>(), new List<object>()));

            var lugar = _lugaresVoo.FirstOrDefault(l => l.IdLugarVoo == _bilheteSelecionado.IdLugarVoo);
            if (lugar != null)
            {
                ComboLugar.SelectedItem = new LugarVooViewModel
                {
                    IdLugarVoo = lugar.IdLugarVoo,
                    Descricao = $"{lugar.Fila}{lugar.Coluna} ({lugar.Tipo})",
                    Tipo = lugar.Tipo
                };
            }
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (ComboVoo.SelectedItem is not VooViewModel vooSelecionado ||
                ComboLugar.SelectedItem is not LugarVooViewModel lugarSelecionado)
            {
                MessageBox.Show("Seleciona um voo e um lugar.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Passageiro passageiroSelecionado = ComboPassageiro.SelectedItem as Passageiro;
            int idPassageiro = 0;

            if (passageiroSelecionado == null)
            {
                var novoPassageiro = new Passageiro
                {
                    Nome = "Default",
                    Apelido = "Default",
                    VoosVoados = 0
                };

                var respostaNovo = await _passageiroService.CriarAsync(novoPassageiro);

                if (respostaNovo.IsSuccess && respostaNovo.Result is Passageiro criado)
                {
                    _passageiros.Add(criado);
                    ComboPassageiro.ItemsSource = null;
                    ComboPassageiro.ItemsSource = _passageiros;
                    ComboPassageiro.SelectedItem = criado;
                    idPassageiro = criado.IdPassageiro;
                }
                else
                {
                    MessageBox.Show("Erro ao criar passageiro padrão.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                idPassageiro = passageiroSelecionado.IdPassageiro;
            }

            decimal precoDecimal = lugarSelecionado.Tipo.Equals("Executivo", StringComparison.OrdinalIgnoreCase)
                ? vooSelecionado.PrecoExecutivo
                : vooSelecionado.PrecoEconomico;

            if (_bilheteSelecionado == null)
            {
                var novoBilhete = new Bilhete
                {
                    IdLugarVoo = lugarSelecionado.IdLugarVoo,
                    IdPassageiro = idPassageiro,
                    Preco = (int)precoDecimal
                };

                var resposta = await _bilheteService.CriarAsync(novoBilhete);
                if (resposta.IsSuccess)
                    MessageBox.Show("Bilhete criado com sucesso.");
                else
                    MessageBox.Show("Erro ao criar bilhete: " + resposta.Message);
            }
            else
            {
                _bilheteSelecionado.IdLugarVoo = lugarSelecionado.IdLugarVoo;
                _bilheteSelecionado.IdPassageiro = idPassageiro;
                _bilheteSelecionado.Preco = (int)precoDecimal;

                var resposta = await _bilheteService.AtualizarAsync(_bilheteSelecionado.IdBilhete, _bilheteSelecionado);
                if (resposta.IsSuccess)
                    MessageBox.Show("Bilhete atualizado com sucesso.");
                else
                    MessageBox.Show("Erro ao atualizar bilhete: " + resposta.Message);

                _bilheteSelecionado = null;
            }

            CarregarDadosAsync();
        }

        private async void Apagar_Click(object sender, RoutedEventArgs e)
        {
            if (TabelaBilhetes.SelectedItem is not BilheteViewModel bilheteVM)
            {
                MessageBox.Show("Seleciona um bilhete para apagar.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var bilhete = _bilhetes.FirstOrDefault(b => b.IdBilhete == bilheteVM.IdBilhete);
            if (bilhete == null)
            {
                MessageBox.Show("Bilhete não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confirmar = MessageBox.Show($"Tem a certeza que deseja apagar o bilhete ID: {bilhete.IdBilhete}?", "Confirmar Apagamento", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirmar != MessageBoxResult.Yes) return;

            var resposta = await _bilheteService.ApagarAsync(bilhete.IdBilhete);
            if (resposta.IsSuccess)
                MessageBox.Show("Bilhete apagado com sucesso.");
            else
                MessageBox.Show("Erro ao apagar bilhete: " + resposta.Message);

            CarregarDadosAsync();
        }
    }
}
