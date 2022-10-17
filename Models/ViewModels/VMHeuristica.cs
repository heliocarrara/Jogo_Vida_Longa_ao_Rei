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
                    this.Valor += estouAjudando ? 0 : 50;
                    this.Valor += tocandoORei ? 0 : 50;
                    this.Valor += estouEmPerigo ? 50 : 0;
                    this.Valor += possoInterceptarRei ? 0 : 200;
                    this.Valor += 20 - DistObjOponente;
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
                    this.Valor =  30 - distanciaDoRei;
                    this.Valor += cercando ? 0 : 50;
                    this.Valor += estouHaUmPasso ? 0 : 200;
                    this.Valor += estouEmPerigo ? 50 : 0;
                    break;
            }
        }
    }
}