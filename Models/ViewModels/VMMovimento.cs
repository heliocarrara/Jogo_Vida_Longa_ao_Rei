using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VLR.Models.Enumerators;

namespace VLR.Models.ViewModels
{
    public class VMMovimento
    {
        public VMCasaTabuleiro CasaAtual { get; set; }

        public TipoMovimento Direcao { get; set; }

        public VMCasaTabuleiro CasaObjetivo { get; set; }


        public VMMovimento()
        {
        }

        public VMMovimento(VMCasaTabuleiro casaObjetivo, TipoMovimento direcao, VMCasaTabuleiro casaAtual)
        {
            this.CasaAtual = casaAtual;
            this.Direcao = direcao;
            this.CasaObjetivo = casaObjetivo;
        }
    }
}