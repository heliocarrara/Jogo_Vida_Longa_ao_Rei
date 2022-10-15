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
                    if (possoInterceptarRei)
                    {
                        this.Valor = 0;
                    }
                    else
                    {
                        //this.Valor = distancia
                        this.Valor += estouAjudando ? 0 : 50;
                        this.Valor += tocandoORei ? 0 : 50;
                        this.Valor += estouEmPerigo ? 50 : 0;
                        this.Valor += 20 - DistObjOponente;
                        this.Valor += possoInterceptarRei ? 0 : 50;
                    }
                    break;
            }
        }

        public VMHeuristica(VMMovimento movimento, int distancia, bool estouEmPerigo, int distanciaPercorrida)
        {
            this.Movimento = movimento;

            var jogador = movimento.CasaAtual.Ocupante.HasValue ? movimento.CasaAtual.Ocupante.Value : movimento.CasaObjetivo.Ocupante.Value;

            switch (jogador)
            {
                case Enumerators.TipoJogador.Rei:
                    this.Valor = distancia;
                    this.Valor += estouEmPerigo ? 100 : 0;
                    this.Valor += distanciaPercorrida;
                    break;
            }
        }

        public VMHeuristica(VMMovimento movimento, bool cercando, bool estouEmPerigo)
        {
            this.Movimento = movimento;

            var jogador = movimento.CasaAtual.Ocupante.HasValue ? movimento.CasaAtual.Ocupante.Value : movimento.CasaObjetivo.Ocupante.Value;

            switch (jogador)
            {
                case Enumerators.TipoJogador.Soldado:
                    this.Valor += cercando ? 0 : 50;
                    this.Valor += estouEmPerigo ? 100 : 0;
                    break;
            }
        }
    }
}