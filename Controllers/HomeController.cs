using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VLR.Models.Enumerators;
using VLR.Models.ViewModels;

namespace VLR.Controllers
{
    public class HomeController : Controller
    {
        const string view = "_PartialTabuleiro";
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Jogar()
        {
            try
            {
                var tabuleiro = new VMTabuleiro();

                return View(view, tabuleiro);
            }
            catch
            {
                return Index();
            }
        }

        public ActionResult ProximoJogador(VMTabuleiro tabuleiro, TipoJogador? jogadorAtual)
        {
            try
            {
                var proximoJogador = GetProximoJogador(jogadorAtual);

                var movimentosPossiveis = GetMovimentosPossiveis(tabuleiro, proximoJogador);

                var melhorMovimento = MelhorMovimento(movimentosPossiveis, proximoJogador);

                //REALIZAR O MOVIMENTO DE ACORDO COM O MELHOR ENCONTRADO ACIMA
                //IMPLEMENTAR A TROCA DE POSIÇÃO

                return View(view, tabuleiro);
            }
            catch
            {
                return Jogar();
            }
        }

        private List<TipoJogador> GerarOrdemJogadores()
        {
            return new List<TipoJogador>() { TipoJogador.Soldado, TipoJogador.Mercenario, TipoJogador.Rei };
        }

        private TipoJogador GetProximoJogador(TipoJogador? jogadorAtual)
        {
            var ordem = GerarOrdemJogadores();

            if (jogadorAtual.HasValue)
            {
                int i = 0;
                while (i < ordem.Count)
                {
                    if (jogadorAtual.Value == ordem[i])
                    {
                        if (i == (ordem.Count - 1))
                        {
                            return ordem[0];
                        }
                        else
                        {
                            return ordem[i + 1];
                        }
                    }

                    i++;
                }
            }

            return ordem.FirstOrDefault();
        }

        private List<VMMovimento> GetMovimentosPossiveis (VMTabuleiro tabuleiro, TipoJogador jogador)
        {
            var movimentos = new List<VMMovimento>();

            var casasComJogador = new List<VMCasaTabuleiro>();

            foreach (var cadaColuna in tabuleiro.Colunas)
            {
                casasComJogador.AddRange(cadaColuna.Casas.Where(x => x.Ocupante.HasValue && x.Ocupante.Value == jogador));
            }

            foreach(var cadaPeca in casasComJogador)
            {
                movimentos.AddRange(GetMovimentosDisponiveisPorPeca(tabuleiro, cadaPeca));
            }

            return movimentos;
        }

        private List<VMMovimento> GetMovimentosDisponiveisPorPeca(VMTabuleiro tabuleiro, VMCasaTabuleiro Peca)
        {
            var movimentos = new List<VMMovimento>();
            var casasDisponiveis = new List<VMCasaTabuleiro>();

            int i = 0, j = 0;

            for (i = Peca.x + 1; i < 11; i++)
            {
                casasDisponiveis.Add(tabuleiro.Colunas[i].Casas[Peca.y]);

                if (tabuleiro.Colunas[i].Casas[Peca.y].Ocupante.HasValue)
                {
                    break;
                }

                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[i].Casas[Peca.y], TipoMovimento.Horizontal, i - Peca.x + 1));
            }

            for (i = Peca.x - 1; i > 0; i--)
            {
                if (tabuleiro.Colunas[i].Casas[Peca.y].Ocupante.HasValue)
                {
                    break;
                }

                casasDisponiveis.Add(tabuleiro.Colunas[i].Casas[Peca.y]);

                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[i].Casas[Peca.y], TipoMovimento.Horizontal, Peca.x - 1 - i));
            }

            for(j = Peca.y + 1; j < 11; j++)
            {
                if (tabuleiro.Colunas[Peca.x].Casas[j].Ocupante.HasValue)
                {
                    break;
                }

                casasDisponiveis.Add(tabuleiro.Colunas[Peca.x].Casas[j]);
                
                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[Peca.x].Casas[j], TipoMovimento.Vertical, j - Peca.y + 1));
            }

            for (j = Peca.y - 1; j > 0; j--)
            {
                if (tabuleiro.Colunas[Peca.x].Casas[j].Ocupante.HasValue)
                {
                    break;
                }

                casasDisponiveis.Add(tabuleiro.Colunas[Peca.x].Casas[j]);


                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[Peca.x].Casas[j], TipoMovimento.Vertical, Peca.y - 1 - i));
            }

            casasDisponiveis.Distinct().ToList();

            return movimentos;
        }

        private VMMovimento MelhorMovimento (List<VMMovimento> movimentos, TipoJogador jogador)
        {
            Random rand = new Random();

            return movimentos[rand.Next(0, movimentos.Count)];
        }
    }
}