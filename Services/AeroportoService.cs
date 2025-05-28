using Aeronautica.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Services
{
    public class AeroportoService
    {
        private readonly ApiService _apiService = new ApiService();

        // POST - Criar novo Aeroporto
        public async Task<Response> CriarAeroportoAsync(Aeroporto aeroporto)
        {
            return await _apiService.PostAsync("Aeroporto", aeroporto);
        }

        // GET - Obter Aeroporto
        public async Task<Response> ObterAeroportoAsync()
        {
            return await _apiService.GetAsync<List<Aeroporto>>($"Aeroporto");
        }

        // GET - Obter Aeroporto pot id 
        public async Task<Response> ObterAeroportoPorIdAsync(int id)
        {
            return await _apiService.GetAsync<List<Aeroporto>>($"Aeroporto/{id}");
        }

        // PUT - Atualizar Aeroporto
        public async Task<Response> AtualizAraeroportoAsync(int id,Aeroporto aeroporto)
        {
            return await _apiService.PutAsync($"Aeroporto/{id}", aeroporto);
        }

        // DELETE - Apagar Aeroporto
        public async Task<Response> ApagarAeroportoAsync(int id)
        {
            return await _apiService.DeleteAsync($"Aeroporto/{id}");
        }
    }
}
