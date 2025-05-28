using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Models
{
    public class LugaresVoo
    {
        public int IdLugarVoo { get; set; }
        public int IdVoo { get; set; }
        public string Fila { get; set; } = string.Empty;
        public int Coluna { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
