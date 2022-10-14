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

        public VMHeuristica(VMMovimento movimento, int distancia, bool estouEmPerigo, bool estouAjudando, bool tocandoORei, int distanciaDoRei, int DistObjOponente)
        {
            this.Movimento = movimento;

            var jogador = movimento.CasaAtual.Ocupante.HasValue ? movimento.CasaAtual.Ocupante.Value : movimento.CasaObjetivo.Ocupante.Value;

            switch (jogador)
            {
                case Enumerators.TipoJogador.Mercenario:
                    this.Valor = (12 - (int)Math.Sqrt(distancia)) * 10;
                    this.Valor += (12 - (int)Math.Sqrt(distanciaDoRei)) * 10;
                    this.Valor += DistObjOponente * 100;

                    this.Valor += estouAjudando ? 10000 : 0;
                    this.Valor += tocandoORei ? 30000 : 0;
                    this.Valor += estouEmPerigo ? -10000 : 0 ;

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
                    this.Valor = (int)Math.Sqrt(distancia);
                    this.Valor += estouEmPerigo ? 100 : 0;
                    break;
            }


        }
    }
}