using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VLR.Models.ViewModels
{
    public class VMHeuristica
    {
        public VMMovimento Movimento { get; set; }
        public int Valor { get; set; }

        public VMHeuristica()
        {
        }

        public VMHeuristica(VMMovimento movimento, int distancia, bool estouEmPerigo, bool estouAjudando, bool tocandoORei, int DistObjOponente)
        {
            this.Movimento = movimento;

            var jogador = movimento.CasaAtual.Ocupante.HasValue ? movimento.CasaAtual.Ocupante.Value : movimento.CasaObjetivo.Ocupante.Value;

            switch (jogador)
            {
                case Enumerators.TipoJogador.Mercenario:
                    this.Valor = distancia;
                    this.Valor += DistObjOponente * 500;
                    this.Valor += !estouAjudando ? 10000 : 0;
                    this.Valor += estouEmPerigo ? 100000 : 0;

                    this.Valor += tocandoORei ? (-this.Valor) : 0;
                    break;
            }
        }

        public VMHeuristica(VMMovimento movimento, int distancia, bool estouEmPerigo)
        {
            this.Movimento = movimento;

            var jogador = movimento.CasaAtual.Ocupante.HasValue ? movimento.CasaAtual.Ocupante.Value : movimento.CasaObjetivo.Ocupante.Value;

            switch (jogador)
            {
                case Enumerators.TipoJogador.Soldado:
                case Enumerators.TipoJogador.Rei:
                    this.Valor = distancia;
                    this.Valor += estouEmPerigo ? 1000 : 0;
                    break;
            }


        }
    }
}