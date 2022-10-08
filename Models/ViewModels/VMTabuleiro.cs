using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VLR.Models.Enumerators;

namespace VLR.Models.ViewModels
{
    public class VMTabuleiro
    {
        public List<VMColuna> Colunas { get; set; }

        public TipoJogador jogadorAtual { get; set; }
        public VMTabuleiro()
        {
        }
    }
}