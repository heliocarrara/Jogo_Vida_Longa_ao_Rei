using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VLR.Models.Enumerators;

namespace VLR.Models.ViewModels
{
    public class VMMovimento
    {
        public VMCasaTabuleiro Casa { get; set; }

        public TipoMovimento Direcao { get; set; }

        public int NumCasasAndadas { get; set; }

        public VMMovimento()
        {
        }

        public VMMovimento(VMCasaTabuleiro casa, TipoMovimento direcao, int NumCasasAndadas)
        {
            this.Casa = casa;
            this.Direcao = direcao;
            this.NumCasasAndadas = NumCasasAndadas;
        }
    }
}