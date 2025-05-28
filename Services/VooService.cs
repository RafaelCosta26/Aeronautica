using Aeronautica.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Services
{
    public class VooService
    {
        private readonly ApiService _apiService = new ApiService();

        // POST - Criar novo voo
        public async Task<Response> CriarVooAsync(Voo voo)
        {
            return await _apiService.PostAsync("voos", voo);
        }

        // GET - Obter voo por ID
        public async Task<Response> ObterVooPorIdAsync(int id)
        {
            return await _apiService.GetAsync<Voo>($"voos/{id}");
        }

        // GET - Obter todos os voos
        public async Task<Response> ObterTodosVoosAsync()
        {
            return await _apiService.GetAsync<List<Voo>>("voos");
        }

        // PUT - Atualizar voo
        public async Task<Response> AtualizarVooAsync(int id, Voo voo)
        {
            return await _apiService.PutAsync($"voos/{id}", voo);
        }

        // DELETE - Apagar voo
        public async Task<Response> ApagarVooAsync(int id)
        {
            return await _apiService.DeleteAsync($"voos/{id}");
        }
    }
}
