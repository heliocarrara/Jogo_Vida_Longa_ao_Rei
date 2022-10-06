using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VLR.Models.Enumerators;

namespace VLR.Models.ViewModels
{
    public class VMTabuleiro
    {
        public List<VMColuna> Colunas { get; }

        public VMTabuleiro()
        {
            this.Colunas = new List<VMColuna>();

            for (int i = 0; i < 11; i++)
            {
                this.Colunas.Add(new VMColuna(i));
            }

            OrganizarPecas();
        }

        void OrganizarPecas()
        {
            //Refúgios
            {
                this.Colunas[0].Casas[0].EhObjetivo = true;
                this.Colunas[10].Casas[0].EhObjetivo = true;
                this.Colunas[0].Casas[10].EhObjetivo = true;
                this.Colunas[10].Casas[10].EhObjetivo = true;
            }

            //Rei
            {
                this.Colunas[5].Casas[5].Ocupante = TipoJogador.Rei;
            }

            //Soldados
            {
                this.Colunas[3].Casas[5].Ocupante = TipoJogador.Soldado;
                this.Colunas[4].Casas[4].Ocupante = TipoJogador.Soldado;
                this.Colunas[4].Casas[5].Ocupante = TipoJogador.Soldado;
                this.Colunas[4].Casas[6].Ocupante = TipoJogador.Soldado;
                this.Colunas[5].Casas[3].Ocupante = TipoJogador.Soldado;
                this.Colunas[5].Casas[4].Ocupante = TipoJogador.Soldado;
                this.Colunas[5].Casas[6].Ocupante = TipoJogador.Soldado;
                this.Colunas[5].Casas[7].Ocupante = TipoJogador.Soldado;
                this.Colunas[6].Casas[4].Ocupante = TipoJogador.Soldado;
                this.Colunas[6].Casas[5].Ocupante = TipoJogador.Soldado;
                this.Colunas[6].Casas[6].Ocupante = TipoJogador.Soldado;
                this.Colunas[7].Casas[5].Ocupante = TipoJogador.Soldado;
            }

            //Mercenários
            {
                this.Colunas[3].Casas[0].Ocupante = TipoJogador.Mercenario;
                this.Colunas[4].Casas[0].Ocupante = TipoJogador.Mercenario;
                this.Colunas[5].Casas[0].Ocupante = TipoJogador.Mercenario;
                this.Colunas[6].Casas[0].Ocupante = TipoJogador.Mercenario;
                this.Colunas[7].Casas[0].Ocupante = TipoJogador.Mercenario;
                this.Colunas[5].Casas[1].Ocupante = TipoJogador.Mercenario;

                this.Colunas[0].Casas[3].Ocupante = TipoJogador.Mercenario;
                this.Colunas[0].Casas[4].Ocupante = TipoJogador.Mercenario;
                this.Colunas[0].Casas[5].Ocupante = TipoJogador.Mercenario;
                this.Colunas[0].Casas[6].Ocupante = TipoJogador.Mercenario;
                this.Colunas[0].Casas[7].Ocupante = TipoJogador.Mercenario;
                this.Colunas[1].Casas[5].Ocupante = TipoJogador.Mercenario;

                this.Colunas[3].Casas[10].Ocupante = TipoJogador.Mercenario;
                this.Colunas[4].Casas[10].Ocupante = TipoJogador.Mercenario;
                this.Colunas[5].Casas[10].Ocupante = TipoJogador.Mercenario;
                this.Colunas[6].Casas[10].Ocupante = TipoJogador.Mercenario;
                this.Colunas[7].Casas[10].Ocupante = TipoJogador.Mercenario;
                this.Colunas[5].Casas[9].Ocupante = TipoJogador.Mercenario;

                this.Colunas[9].Casas[5].Ocupante = TipoJogador.Mercenario;
                this.Colunas[10].Casas[3].Ocupante = TipoJogador.Mercenario;
                this.Colunas[10].Casas[4].Ocupante = TipoJogador.Mercenario;
                this.Colunas[10].Casas[5].Ocupante = TipoJogador.Mercenario;
                this.Colunas[10].Casas[6].Ocupante = TipoJogador.Mercenario;
                this.Colunas[10].Casas[7].Ocupante = TipoJogador.Mercenario;
            }
        }
    }
}