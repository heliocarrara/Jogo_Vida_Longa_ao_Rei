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

        public VMTabuleiro(List<VMColuna> colunas, TipoJogador jogadorAtual)
        {
            Colunas = colunas;
            this.jogadorAtual = jogadorAtual;

            this.Colunas = new List<VMColuna>();

            foreach (var cadaColuna in colunas)
            {
                var coluna = new VMColuna();
                
                var listaCasas = new List<VMCasaTabuleiro> ();

                foreach(var cadaCasa in cadaColuna.Casas)
                {
                    listaCasas.Add(new VMCasaTabuleiro { x = cadaCasa.x, y = cadaCasa.y, Ocupante = cadaCasa.Ocupante });
                }

                coluna.Casas = listaCasas;

                this.Colunas.Add(coluna);
            }
        }
    }
}