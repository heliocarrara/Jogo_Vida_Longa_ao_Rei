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

        public VMHeuristica(VMMovimento movimento, bool estouEmPerigo, bool estouAjudando, bool tocandoORei, bool possoInterceptarRei, int DistObjOponente)
        {
            this.Movimento = movimento;

            var jogador = movimento.CasaAtual.Ocupante.HasValue ? movimento.CasaAtual.Ocupante.Value : movimento.CasaObjetivo.Ocupante.Value;

            switch (jogador)
            {
                case Enumerators.TipoJogador.Mercenario:
                    //this.Valor = DistObjOponente;

                    this.Valor += possoInterceptarRei ? 1000 : 0;

                    this.Valor += estouAjudando ? 100 : 0;

                    this.Valor -= estouEmPerigo ? 100 : 0;

                    this.Valor += tocandoORei ? 100 : 0;
                    break;
            }
        }

        public VMHeuristica(VMMovimento movimento, int distanciaPercorrida, bool estouEmPerigo, int distanciaObjetivo)
        {
            this.Movimento = movimento;

            var jogador = movimento.CasaAtual.Ocupante.HasValue ? movimento.CasaAtual.Ocupante.Value : movimento.CasaObjetivo.Ocupante.Value;

            switch (jogador)
            {
                case Enumerators.TipoJogador.Rei:
                    //this.Valor = distanciaPercorrida;
                    this.Valor += estouEmPerigo ? 100 : 0;
                    this.Valor += distanciaObjetivo;
                    break;
            }
        }

        public VMHeuristica(VMMovimento movimento, bool cercando, bool estouEmPerigo, int distanciaDoRei, bool estouHaUmPasso)
        {
            this.Movimento = movimento;

            var jogador = movimento.CasaAtual.Ocupante.HasValue ? movimento.CasaAtual.Ocupante.Value : movimento.CasaObjetivo.Ocupante.Value;

            switch (jogador)
            {
                case Enumerators.TipoJogador.Soldado:
                    this.Valor = distanciaDoRei;
                    this.Valor += estouEmPerigo ? (distanciaDoRei * 2) : 0;
                    this.Valor -= estouHaUmPasso ? ((distanciaDoRei / 100) * 50) : 0;
                    this.Valor -= cercando ? ((distanciaDoRei / 100) * 20) : 0;
                    break;
            }
        }
    }
}