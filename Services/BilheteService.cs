using Aeronautica.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Services
{
    public class BilheteService
    {
        private readonly ApiService _apiService = new ApiService();

        public async Task<Response> ObterTodosAsync()
            => await _apiService.GetAsync<List<Bilhete>>("bilhetes");

        public async Task<Response> ObterPorIdAsync(int id)
            => await _apiService.GetAsync<Bilhete>($"bilhetes/{id}");

        public async Task<Response> CriarAsync(Bilhete bilhete)
            => await _apiService.PostAsync("bilhetes", bilhete);

        public async Task<Response> AtualizarAsync(int id, Bilhete bilhete)
            => await _apiService.PutAsync($"bilhetes/{id}", bilhete);

        public async Task<Response> ApagarAsync(int id)
            => await _apiService.DeleteAsync($"bilhetes/{id}");
    }
}
