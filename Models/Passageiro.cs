using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeronautica.Models
{
    public class Passageiro
    {
        public int IdPassageiro { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Apelido { get; set; } = string.Empty;
        public int VoosVoados { get; set; }

        public string NomeCompleto => $"{Nome} {Apelido}";
    }
}
