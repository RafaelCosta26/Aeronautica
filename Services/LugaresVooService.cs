using Aeronautica.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Services
{
    public class LugaresVooService
    {
        private readonly ApiService _apiService = new ApiService();

        public async Task<Response> ObterTodosAsync()
            => await _apiService.GetAsync<List<LugaresVoo>>("lugaresvoo");

        public async Task<Response> GerarNovosLugaresParaVooAsync(int id, Voo voo)
            => await _apiService.PutAsync<Voo>($"lugaresvoo/{id}", voo);

        public async Task<Response> ApagarAsync(int id)
            => await _apiService.DeleteAsync($"lugaresvoo/{id}");
    }
}
