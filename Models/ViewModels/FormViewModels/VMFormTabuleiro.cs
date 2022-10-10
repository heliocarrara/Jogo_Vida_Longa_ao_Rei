using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VLR.Models.ViewModels.FormViewModels
{
    public class VMFormTabuleiro
    {
        public int qntMovimento { get; set; }
        public string pecas { get; set; }
        public VMTabuleiro Tabuleiro { get; set; }

        public List<string> mensagens { get; set; }

        public VMFormTabuleiro()
        {
            this.mensagens = new List<string>();
        }

        //public VMFormTabuleiro(VMTabuleiro tabuleiro, int qntMovimento) : this()
        //{
        //    this.Tabuleiro = tabuleiro;
        //    this.qntMovimento = qntMovimento;

        //    this.jogadorAtual = (int)this.Tabuleiro.jogadorAtual;
        //}
    }
}