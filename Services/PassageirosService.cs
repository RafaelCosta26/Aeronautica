using Aeronautica.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Services
{
    public class PassageirosService
    {
        private readonly ApiService _apiService = new ApiService();

        public async Task<Response> ObterTodosAsync()
            => await _apiService.GetAsync<List<Passageiro>>("passagers");

        public async Task<Response> ObterPorIdAsync(int id)
            => await _apiService.GetAsync<Passageiro>($"passagers/{id}");

        public async Task<Response> CriarAsync(Passageiro passageiro)
            => await _apiService.PostAsync("passagers", passageiro);

        public async Task<Response> AtualizarAsync(int id, Passageiro passageiro)
            => await _apiService.PutAsync($"passagers/{id}", passageiro);

        public async Task<Response> ApagarAsync(int id)
            => await _apiService.DeleteAsync($"passagers/{id}");
    }
}
