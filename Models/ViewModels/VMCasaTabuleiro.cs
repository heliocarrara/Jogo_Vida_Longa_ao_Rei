using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VLR.Models.Enumerators;

namespace VLR.Models.ViewModels
{
    public class VMCasaTabuleiro
    {
        public TipoJogador? Ocupante { get; set; }
        public bool EhObjetivo { get; set; }
        public int x { get; set; }
        public int y { get; set; }



        public VMCasaTabuleiro()
        {
        }

        public VMCasaTabuleiro(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}