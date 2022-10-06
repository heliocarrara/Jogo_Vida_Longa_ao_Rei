using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VLR.Models.ViewModels
{
    public class VMColuna
    {
        public char Rotulo { get; set; }
        public List<VMCasaTabuleiro> Casas { get; }

        public VMColuna (int x)
        {
            this.Casas = new List<VMCasaTabuleiro>();

            for (int y =0; y < 11; y++)
            {
                this.Rotulo = (char)y;
                this.Casas.Add(new VMCasaTabuleiro(x, y));

            }
        }
    }
}