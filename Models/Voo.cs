using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Models
{
    public class Voo
    {
        public int IdVoo { get; set; }
        public int IdAeroportoPartida { get; set; }
        public int IdAeroportoDestino { get; set; }
        public int IdAviao { get; set; }
        public string DataPartida { get; set; }
        public string HoraPartida { get; set; }
        public string DataChegada { get; set; }
        public string HoraChegada { get; set; }
        public string RefeicaoIncluida { get; set; }
        public decimal PrecoEconomico { get; set; }
        public decimal PrecoExecutivo { get; set; }
    }
}
