using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VLR.Models.Enumerators;
using VLR.Models.ViewModels;
using VLR.Models.ViewModels.FormViewModels;

namespace VLR.Controllers
{
    public class HomeController : Controller
    {
        const string view = "_PartialTabuleiro";
        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult Jogar()
        {
            try
            {
                var tabuleiro = OrganizarPecas(GerarMatriz());
                var form = new VMFormTabuleiro();

                form.Tabuleiro = tabuleiro;
                form.pecas = CodificarPecas(form.Tabuleiro);
                form.qntMovimento = 1;

                form.mensagens.Add("Tabuleiro Criado e peças organizadas.");
                form.mensagens.Add("Jogador Atual: " + (int)form.Tabuleiro.jogadorAtual);

                return View(view, form);
            }
            catch
            {
                return Index();
            }
        }

        public ActionResult ProximoJogador(VMFormTabuleiro form)
        {
            try
            {
                //var form = new VMFormTabuleiro();

                if (form == null || string.IsNullOrEmpty(form.pecas))
                {
                    throw new Exception("Formularío não carregado.");
                }

                form.Tabuleiro = GerarMatriz();

                form.Tabuleiro = DecodificarPecas(form.Tabuleiro, form.pecas);
                form.mensagens.Add("Peças decodificadas.");

                form.qntMovimento = (form.qntMovimento + 1);
                form.mensagens.Add("Jogador Atual: " + (int)form.Tabuleiro.jogadorAtual);

                var proximoJogador = GetProximoJogador(form.Tabuleiro.jogadorAtual);

                var movimentosPossiveis = GetMovimentosPossiveis(form.Tabuleiro, proximoJogador);

                if (movimentosPossiveis.Any())
                {
                    var melhorMovimento = MelhorMovimento(movimentosPossiveis, proximoJogador);

                    form.Tabuleiro = RealizarMovimento(form.Tabuleiro, melhorMovimento);
                }
                else
                {
                    form.Tabuleiro.jogadorAtual = proximoJogador;
                }

                form.pecas = CodificarPecas(form.Tabuleiro);

                return View(view, form);
            }
            catch (Exception ex)
            {
                return Index();
            }
        }

        public ActionResult Proximo(string pecas)
        {
            var form = new VMFormTabuleiro();

            form.pecas = pecas;

            return ProximoJogador(form);
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

        private List<VMMovimento> GetMovimentosPossiveis(VMTabuleiro tabuleiro, TipoJogador jogador)
        {
            var movimentos = new List<VMMovimento>();

            var casasComJogador = new List<VMCasaTabuleiro>();

            foreach (var cadaColuna in tabuleiro.Colunas)
            {
                casasComJogador.AddRange(cadaColuna.Casas.Where(x => x.Ocupante.HasValue && x.Ocupante.Value == jogador));
            }

            casasComJogador = casasComJogador.Distinct().ToList();

            foreach (var cadaPeca in casasComJogador)
            {
                movimentos.AddRange(GetMovimentosDisponiveisPorPeca(tabuleiro, cadaPeca));
            }

            return movimentos;
        }

        private List<VMMovimento> GetMovimentosDisponiveisPorPeca(VMTabuleiro tabuleiro, VMCasaTabuleiro Peca)
        {
            var movimentos = new List<VMMovimento>();

            int i = 0, j = 0;

            for (i = Peca.x + 1; i < 11; i++)
            {
                if (tabuleiro.Colunas[i].Casas[Peca.y].Ocupante.HasValue)
                {
                    break;
                }

                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[i].Casas[Peca.y], TipoMovimento.Horizontal, Peca));
            }

            for (i = Peca.x - 1; i > 0; i--)
            {
                if (tabuleiro.Colunas[i].Casas[Peca.y].Ocupante.HasValue)
                {
                    break;
                }

                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[i].Casas[Peca.y], TipoMovimento.Horizontal, Peca));
            }

            for (j = Peca.y + 1; j < 11; j++)
            {
                if (tabuleiro.Colunas[Peca.x].Casas[j].Ocupante.HasValue)
                {
                    break;
                }

                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[Peca.x].Casas[j], TipoMovimento.Vertical, Peca));
            }

            for (j = Peca.y - 1; j > 0; j--)
            {
                if (tabuleiro.Colunas[Peca.x].Casas[j].Ocupante.HasValue)
                {
                    break;
                }

                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[Peca.x].Casas[j], TipoMovimento.Vertical, Peca));
            }

            return movimentos.Distinct().ToList();
        }

        private VMMovimento MelhorMovimento(List<VMMovimento> movimentos, TipoJogador jogador)
        {
            Random rand = new Random();

            return movimentos[rand.Next(0, movimentos.Count)];
        }

        private VMTabuleiro RealizarMovimento(VMTabuleiro tabuleiro, VMMovimento movimento)
        {
            tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante = movimento.CasaAtual.Ocupante.Value;
            tabuleiro.Colunas[movimento.CasaAtual.x].Casas[movimento.CasaAtual.y].Ocupante = null;

            tabuleiro.jogadorAtual = movimento.CasaObjetivo.Ocupante.Value;

            return tabuleiro;
        }

        private VMTabuleiro OrganizarPecas(VMTabuleiro tabuleiro)
        {
            tabuleiro.jogadorAtual = TipoJogador.Rei;
            //Refúgios
            {
                tabuleiro.Colunas[0].Casas[0].EhObjetivo = true;
                tabuleiro.Colunas[10].Casas[0].EhObjetivo = true;
                tabuleiro.Colunas[0].Casas[10].EhObjetivo = true;
                tabuleiro.Colunas[10].Casas[10].EhObjetivo = true;
            }

            //Rei
            {
                tabuleiro.Colunas[5].Casas[5].Ocupante = TipoJogador.Rei;
            }

            //Soldados
            {
                tabuleiro.Colunas[3].Casas[5].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[4].Casas[4].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[4].Casas[5].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[4].Casas[6].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[5].Casas[3].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[5].Casas[4].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[5].Casas[6].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[5].Casas[7].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[6].Casas[4].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[6].Casas[5].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[6].Casas[6].Ocupante = TipoJogador.Soldado;
                tabuleiro.Colunas[7].Casas[5].Ocupante = TipoJogador.Soldado;
            }

            //Mercenários
            {
                tabuleiro.Colunas[3].Casas[0].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[4].Casas[0].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[5].Casas[0].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[6].Casas[0].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[7].Casas[0].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[5].Casas[1].Ocupante = TipoJogador.Mercenario;

                tabuleiro.Colunas[0].Casas[3].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[0].Casas[4].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[0].Casas[5].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[0].Casas[6].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[0].Casas[7].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[1].Casas[5].Ocupante = TipoJogador.Mercenario;

                tabuleiro.Colunas[3].Casas[10].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[4].Casas[10].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[5].Casas[10].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[6].Casas[10].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[7].Casas[10].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[5].Casas[9].Ocupante = TipoJogador.Mercenario;

                tabuleiro.Colunas[9].Casas[5].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[10].Casas[3].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[10].Casas[4].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[10].Casas[5].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[10].Casas[6].Ocupante = TipoJogador.Mercenario;
                tabuleiro.Colunas[10].Casas[7].Ocupante = TipoJogador.Mercenario;
            }

            return tabuleiro;
        }

        private VMTabuleiro GerarMatriz()
        {
            var tabuleiro = new VMTabuleiro();

            tabuleiro.Colunas = new List<VMColuna>();

            for (int i = 0; i < 11; i++)
            {
                tabuleiro.Colunas.Add(new VMColuna());
                tabuleiro.Colunas[i].Casas = new List<VMCasaTabuleiro>();

                for (int j = 0; j < 11; j++)
                {
                    tabuleiro.Colunas[i].Casas.Add(new VMCasaTabuleiro());

                    tabuleiro.Colunas[i].Casas[j].x = i;
                    tabuleiro.Colunas[i].Casas[j].y = j;
                }
            }

            return tabuleiro;
        }

        private string CodificarPecas(VMTabuleiro tabuleiro)
        {
            string pecas = string.Empty;

            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    int p = tabuleiro.Colunas[i].Casas[j].Ocupante.HasValue ? (int)tabuleiro.Colunas[i].Casas[j].Ocupante.Value : 0;
                    pecas += i + "," + j + "," + p + ";";
                }
            }

            pecas += (int)tabuleiro.jogadorAtual;

            return pecas;
        }

        private VMTabuleiro DecodificarPecas(VMTabuleiro tabuleiro, string pecas)
        {
            var p = pecas.Split(';').ToList();

            tabuleiro.jogadorAtual = (TipoJogador)int.Parse(p[p.Count - 1]);

            foreach (var cadaItem in p)
            {
                if (!string.IsNullOrWhiteSpace(cadaItem))
                {
                    var cadaPeca = cadaItem.Split(',');

                    if (cadaPeca.Length > 2 && int.Parse(cadaPeca[2]) != 0)
                    {
                        tabuleiro.Colunas[int.Parse(cadaPeca[0])].Casas[int.Parse(cadaPeca[1])].Ocupante = (TipoJogador)int.Parse(cadaPeca[2]);
                    }//
                    //TipoJogador? tipoJogador = null;

                    //if (int.Parse(cadaPeca[2]) != 0)
                    //{
                    //    tipoJogador = (TipoJogador)int.Parse(cadaPeca[2]);
                    //}

                    //tabuleiro.Colunas[int.Parse(cadaPeca[0])].Casas[int.Parse(cadaPeca[1])].Ocupante = tipoJogador;


                }
            }

            return tabuleiro;
        }
    }
}