using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLR.Models.Enumerators
{
    public enum TipoJogador
    {
        [Description("Rei")]
        Rei = 1,

        [Description("Soldado")]
        Soldado = 2,

        [Description("Mercenário")]
        Mercenario = 3
    }
}
