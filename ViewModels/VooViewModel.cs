using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.ViewModels
{
    public  class VooViewModel
    {
        public int IdVoo { get; set; }
        public string AeroportoPartida { get; set; }
        public string AeroportoDestino { get; set; }
        public string Aviao { get; set; }
        public string DataPartida { get; set; }
        public string HoraPartida { get; set; }
        public string DataChegada { get; set; }
        public string HoraChegada { get; set; }
        public string RefeicaoIncluida { get; set; }
        public decimal PrecoEconomico { get; set; }
        public decimal PrecoExecutivo { get; set; }

        public string Nome
        {
            get
            {
                return $"{AeroportoPartida} - {AeroportoDestino}";
            }
        }

    }
}
