using Aeronautica.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Services
{
    public class AvioeService
    {


        private readonly ApiService _apiService = new ApiService();

        // GET todos os aviões
        public async Task<Response> ObterTodosAsync()
        {
            return await _apiService.GetAsync<List<Avioe>>("avioes");
        }

        // GET por ID
        public async Task<Response> ObterPorIdAsync(int id)
        {
            return await _apiService.GetAsync<Avioe>($"avioes/{id}");
        }

        // POST
        public async Task<Response> CriarAsync(Avioe aviao)
        {
            return await _apiService.PostAsync("avioes", aviao);
        }

        // PUT
        public async Task<Response> AtualizarAsync(int id, Avioe aviao)
        {
            return await _apiService.PutAsync($"avioes/{id}", aviao);
        }

        // DELETE
        public async Task<Response> ApagarAsync(int id)
        {
            return await _apiService.DeleteAsync($"avioes/{id}");
        }
    }
        
}
