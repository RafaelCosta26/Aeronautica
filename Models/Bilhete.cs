using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Models
{
    public class Bilhete
    {
        public int IdBilhete { get; set; }
        public int IdPassageiro { get; set; }
        public int IdLugarVoo { get; set; }
        public decimal Preco { get; set; }
    }
}
