using Aeronautica.Models;
using Aeronautica.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Services
{
    public class LugaresService
    {
        private readonly ApiService _apiService = new ApiService();

        public async Task<Response> ApagarAsync(int idAviao)
        {
          return await _apiService.DeleteAsync($"avioes/apagarLugares/{idAviao}");
        }

        public async Task<Response> GerarLugaresParaAviaoAsync(GerarLugaresRequest request)
        {
            return await _apiService.PostAsync("avioes/gerarLugares", request);
        }
    }
}
