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

                string aux;
                if (ObjetivoConcluido(form.Tabuleiro, out aux))
                {
                    form.mensagens.Add(aux);
                    form.pecas = CodificarPecas(form.Tabuleiro);
                    return View(view, form);
                }

                form.qntMovimento = (form.qntMovimento + 1);

                var proximoJogador = GetProximoJogador(form.Tabuleiro.jogadorAtual);

                form.Tabuleiro.jogadorAtual = proximoJogador;

                var melhorMovimento = MelhorMovimento(form.Tabuleiro);

                if (melhorMovimento != null)
                {
                    List<string> message;
                    form.mensagens.Add(DescreverPasso(melhorMovimento));

                    form.Tabuleiro = RealizarMovimento(form.Tabuleiro, melhorMovimento, out message);

                    if (message.Any())
                    {
                        form.mensagens.AddRange(message);
                    }
                }
                else
                {
                    form.mensagens.Add("Sem movimentos disponíveis para o " + GetTipoJogadorString(proximoJogador));
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

        private List<VMMovimento> GetMovimentosPossiveis(VMTabuleiro tabuleiro)
        {
            var movimentos = new List<VMMovimento>();

            var casasComJogador = new List<VMCasaTabuleiro>();

            foreach (var cadaColuna in tabuleiro.Colunas)
            {
                casasComJogador.AddRange(cadaColuna.Casas.Where(x => x.Ocupante.HasValue && x.Ocupante.Value == tabuleiro.jogadorAtual));
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
                if (tabuleiro.Colunas[i].Casas[Peca.y].Ocupante.HasValue || (tabuleiro.Colunas[i].Casas[Peca.y].EhObjetivo && tabuleiro.jogadorAtual != TipoJogador.Rei))
                {
                    break;
                }

                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[i].Casas[Peca.y], TipoMovimento.Horizontal, Peca));
            }

            for (i = Peca.x - 1; i > 0; i--)
            {
                if (tabuleiro.Colunas[i].Casas[Peca.y].Ocupante.HasValue || (tabuleiro.Colunas[i].Casas[Peca.y].EhObjetivo && tabuleiro.jogadorAtual != TipoJogador.Rei))
                {
                    break;
                }

                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[i].Casas[Peca.y], TipoMovimento.Horizontal, Peca));
            }

            for (j = Peca.y + 1; j < 11; j++)
            {
                if (tabuleiro.Colunas[Peca.x].Casas[j].Ocupante.HasValue || (tabuleiro.Colunas[Peca.x].Casas[j].EhObjetivo && tabuleiro.jogadorAtual != TipoJogador.Rei))
                {
                    break;
                }

                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[Peca.x].Casas[j], TipoMovimento.Vertical, Peca));
            }

            for (j = Peca.y - 1; j > 0; j--)
            {
                if (tabuleiro.Colunas[Peca.x].Casas[j].Ocupante.HasValue || (tabuleiro.Colunas[Peca.x].Casas[j].EhObjetivo && tabuleiro.jogadorAtual != TipoJogador.Rei))
                {
                    break;
                }

                //REVISAR A DISTANCIA
                movimentos.Add(new VMMovimento(tabuleiro.Colunas[Peca.x].Casas[j], TipoMovimento.Vertical, Peca));
            }

            return movimentos.Distinct().ToList();
        }

        private VMMovimento MelhorMovimento(VMTabuleiro tabuleiro)
        {

            var movimentosPossiveis = GetMovimentosPossiveis(tabuleiro);

            if (!movimentosPossiveis.Any())
            {
                return null;
            }

            var rand = new Random();
            int c = rand.Next(0, movimentosPossiveis.Count);

            if (tabuleiro.jogadorAtual == TipoJogador.Mercenario)
            {
                return movimentosPossiveis[c];
            }

            int posicao = c;

            switch (tabuleiro.jogadorAtual)
            {
                case TipoJogador.Soldado:
                    //ReiEstaProtegido()
                    break;
                case TipoJogador.Rei:
                    //Get4Objetivos()
                    break;
                default:
                    break;
            }

            foreach (var cadaMovimento in movimentosPossiveis)
            {

                //EstouAjudandoACercar();
                //EstouEmPerigo()
                //PossoAndarAqui Deveria estar em movimentos disponiveis


            }






            return movimentosPossiveis[posicao];
        }

        private VMTabuleiro RealizarMovimento(VMTabuleiro tabuleiro, VMMovimento movimento, out List<string> message)
        {
            message = new List<string>();

            tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante = movimento.CasaAtual.Ocupante.Value;
            tabuleiro.Colunas[movimento.CasaAtual.x].Casas[movimento.CasaAtual.y].Ocupante = null;

            tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].movida = true;
            tabuleiro.Colunas[movimento.CasaAtual.x].Casas[movimento.CasaAtual.y].movida = true;

            tabuleiro.jogadorAtual = movimento.CasaObjetivo.Ocupante.Value;

            var adjacentes = GetCasasAdjacentes(tabuleiro, movimento.CasaObjetivo);

            var guardaReal = new List<TipoJogador> { TipoJogador.Rei, TipoJogador.Soldado };

            //Quando anda para armadilha
            {
                if (adjacentes.Count(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario) > 1 && tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante == TipoJogador.Soldado)
                {
                    message.Add(string.Format("Um Soldado foi eliminado em: ({0},{1})", movimento.CasaObjetivo.x, movimento.CasaObjetivo.y));
                    tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante = null;
                }

                if (adjacentes.Count(x => x.Ocupante.HasValue && guardaReal.Contains(x.Ocupante.Value)) > 1 && tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante == TipoJogador.Mercenario)
                {
                    message.Add(string.Format("Um mercenário foi eliminado: ({0},{1})", movimento.CasaObjetivo.x, movimento.CasaObjetivo.y));
                    tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante = null;
                }

                if (tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante == TipoJogador.Rei && adjacentes.Count(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario) > 4)
                {
                    message.Add(string.Format("O Rei morreu em: ({0},{1})", movimento.CasaObjetivo.x, movimento.CasaObjetivo.y));
                    tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante = null;
                }
            }

            //Quando vai ajudar a eliminar
            {
                foreach (var cadaAdjacente in adjacentes)
                {
                    if (cadaAdjacente.Ocupante.HasValue)
                    {
                        var subAdjacentes = GetCasasAdjacentes(tabuleiro, cadaAdjacente);

                        if (subAdjacentes.Count(x => x.Ocupante.HasValue && guardaReal.Contains(x.Ocupante.Value)) > 1 && cadaAdjacente.Ocupante.Value == TipoJogador.Mercenario)
                        {
                            message.Add(string.Format("Um Mercenário foi encurralado em: ({0},{1})", cadaAdjacente.x, cadaAdjacente.y));
                            tabuleiro.Colunas[cadaAdjacente.x].Casas[cadaAdjacente.y].Ocupante = null;
                        }

                        if (subAdjacentes.Count(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario) > 1 && cadaAdjacente.Ocupante.Value == TipoJogador.Soldado)
                        {
                            message.Add(string.Format("Um Soldado foi encurralado em: ({0},{1})", cadaAdjacente.x, cadaAdjacente.y));
                            tabuleiro.Colunas[cadaAdjacente.x].Casas[cadaAdjacente.y].Ocupante = null;
                        }

                        if (subAdjacentes.Count(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario) > 3 && cadaAdjacente.Ocupante.Value == TipoJogador.Rei)
                        {
                            message.Add(string.Format("O Rei foi encurralado em: ({0},{1})", cadaAdjacente.x, cadaAdjacente.y));
                            tabuleiro.Colunas[cadaAdjacente.x].Casas[cadaAdjacente.y].Ocupante = null;
                        }
                    }
                }
            }

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

            tabuleiro.Colunas[0].Casas[0].EhObjetivo = true;
            tabuleiro.Colunas[0].Casas[10].EhObjetivo = true;
            tabuleiro.Colunas[10].Casas[0].EhObjetivo = true;
            tabuleiro.Colunas[10].Casas[10].EhObjetivo = true;

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

        private string DescreverPasso(VMMovimento movimento)
        {
            return string.Format("O {0} foi de: ({1},{2}) --> ({3},{4})", GetTipoJogadorString(movimento.CasaAtual.Ocupante.Value), movimento.CasaAtual.x.ToString(), movimento.CasaAtual.y.ToString(), movimento.CasaObjetivo.x.ToString(), movimento.CasaObjetivo.y.ToString());
        }
        private string GetTipoJogadorString(TipoJogador tipo)
        {
            switch (tipo)
            {
                case TipoJogador.Mercenario:
                    return "Mercenário";

                case TipoJogador.Rei:
                    return "Rei";
                case TipoJogador.Soldado:
                    return "Soldado";
                default:
                    return "(Erro ao carregar tipo)";
            }
        }

        private List<VMCasaTabuleiro> GetCasasAdjacentes(VMTabuleiro tabuleiro, VMCasaTabuleiro CasaObjetivo)
        {
            var adjacentes = new List<VMCasaTabuleiro>();

            if (CasaObjetivo.x > 0)
            {
                adjacentes.Add(tabuleiro.Colunas[CasaObjetivo.x - 1].Casas[CasaObjetivo.y]);
            }

            if (CasaObjetivo.x < 10)
            {
                adjacentes.Add(tabuleiro.Colunas[CasaObjetivo.x + 1].Casas[CasaObjetivo.y]);
            }

            if (CasaObjetivo.y > 0)
            {
                adjacentes.Add(tabuleiro.Colunas[CasaObjetivo.x].Casas[CasaObjetivo.y - 1]);
            }

            if (CasaObjetivo.y < 10)
            {
                adjacentes.Add(tabuleiro.Colunas[CasaObjetivo.x].Casas[CasaObjetivo.y + 1]);
            }

            return adjacentes;
        }

        private bool ObjetivoConcluido(VMTabuleiro tabuleiro, out string message)
        {
            message = "Objetivo concluído por: " + GetTipoJogadorString(tabuleiro.jogadorAtual);

            if (!tabuleiro.Colunas.Any(y => y.Casas.Any(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Rei)) && tabuleiro.jogadorAtual == TipoJogador.Mercenario)
            {
                return true;
            }

            var objetivos = new List<VMCasaTabuleiro>() { tabuleiro.Colunas[0].Casas[0], tabuleiro.Colunas[10].Casas[0], tabuleiro.Colunas[0].Casas[10], tabuleiro.Colunas[10].Casas[10] };

            if(objetivos.Any(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Rei))
            {
                return true;
            }

            message = "Objetivo ainda não concluído por: " + GetTipoJogadorString(tabuleiro.jogadorAtual);
            return false;
        }
    }
}