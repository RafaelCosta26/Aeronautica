using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.ViewModels
{
    public class BilheteViewModel
    {
        public int IdBilhete { get; set; }
        public string NomePassageiro { get; set; }
        public string LugarDescricao { get; set; }
        public int Preco { get; set; }
    }
}
