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

                var melhorMovimento = MelhorMovimento(form.Tabuleiro,0, out int i);

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
            try
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
            catch (Exception ex)
            {
                return TipoJogador.Rei;
            }
        }

        private List<VMMovimento> GetMovimentosPossiveis(VMTabuleiro tabuleiro)
        {
            try
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
            catch (Exception ex)
            {
                return null;
            }

        }

        private List<VMMovimento> GetMovimentosDisponiveisPorPeca(VMTabuleiro tabuleiro, VMCasaTabuleiro Peca)
        {
            try
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

                for (i = Peca.x - 1; i >= 0; i--)
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

                for (j = Peca.y - 1; j >= 0; j--)
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
            catch (Exception ex)
            {
                return null;
            }
        }

        private VMMovimento MelhorMovimento(VMTabuleiro tabuleiro, int level, out int nivel)
        {
            try
            {
                nivel = level++;
                var movimentosPossiveis = GetMovimentosPossiveis(tabuleiro);

                if (ObjetivoConcluido(tabuleiro, out string m))
                {
                    return null;
                }

                var listaMovimentos = new List<VMHeuristica>();

                if (!movimentosPossiveis.Any())
                {
                    return null;
                }

                var rand = new Random();
                int c = rand.Next(0, movimentosPossiveis.Count);

                VMCasaTabuleiro melhorObjetivo;

                List<string> messages;
                switch (tabuleiro.jogadorAtual)
                {
                    case TipoJogador.Soldado:
                        var movimentos = AbrirCaminho(tabuleiro);

                        if (movimentos == null || !movimentos.Any())
                        {
                            listaMovimentos.Add(new VMHeuristica(movimentosPossiveis[c], 0, EstouEmPerigo(tabuleiro, movimentosPossiveis[c].CasaObjetivo)));
                        }
                        else
                        {
                            listaMovimentos.AddRange(movimentos);
                        }

                        listaMovimentos = listaMovimentos.OrderBy(y => y.Valor).ToList();
                        break;
                    case TipoJogador.Rei:
                        melhorObjetivo = MelhorObjetivo(tabuleiro, TipoJogador.Rei);

                        for (var z = 0; z < movimentosPossiveis.Count; z++)
                        {
                            listaMovimentos.Add(new VMHeuristica(movimentosPossiveis[z], CalcularDistancia(melhorObjetivo, movimentosPossiveis[z].CasaObjetivo), EstouEmPerigo(tabuleiro, movimentosPossiveis[z].CasaObjetivo)));
                        }

                        listaMovimentos = listaMovimentos.OrderBy(y => y.Valor).ToList();
                        break;
                    case TipoJogador.Mercenario:
                        melhorObjetivo = MelhorObjetivo(tabuleiro, TipoJogador.Mercenario);

                        for (var z = 0; z < movimentosPossiveis.Count; z++)
                        {
                            var tabuleiroSimulado = SimularJogada(tabuleiro, movimentosPossiveis[z]);

                            tabuleiroSimulado.jogadorAtual = TipoJogador.Rei;

                            var melhorMovimentoRei = MelhorMovimento(tabuleiroSimulado, level, out nivel);

                            listaMovimentos.Add(new VMHeuristica(movimentosPossiveis[z], CalcularDistancia(movimentosPossiveis[z].CasaAtual, melhorObjetivo), EstouEmPerigo(tabuleiro, movimentosPossiveis[z].CasaObjetivo), EstouAjudandoACercar(tabuleiro, movimentosPossiveis[z].CasaObjetivo), PossoTocarORei(tabuleiro, movimentosPossiveis[z]), level));
                        }

                        listaMovimentos = listaMovimentos.OrderBy(y => y.Valor).ToList();
                        break;
                    default:
                        listaMovimentos.Add(new VMHeuristica(movimentosPossiveis[c], 0, false, false, false, 0));
                        break;
                }



                return listaMovimentos[0].Movimento;
            }
            catch (Exception ex)
            {
                nivel = level;
                return null;
            }
        }

        private VMTabuleiro RealizarMovimento(VMTabuleiro tabuleiro, VMMovimento movimento, out List<string> message)
        {
            try
            {

                message = new List<string>();

                tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante = movimento.CasaAtual.Ocupante.Value;
                tabuleiro.Colunas[movimento.CasaAtual.x].Casas[movimento.CasaAtual.y].Ocupante = null;

                tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].movida = true;
                tabuleiro.Colunas[movimento.CasaAtual.x].Casas[movimento.CasaAtual.y].movida = true;

                tabuleiro.jogadorAtual = tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante.Value;

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

                    //if (tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante == TipoJogador.Rei && adjacentes.Count(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario) == adjacentes.Count)
                    //{
                    //    message.Add(string.Format("O Rei morreu em: ({0},{1})", movimento.CasaObjetivo.x, movimento.CasaObjetivo.y));
                    //    tabuleiro.Colunas[movimento.CasaObjetivo.x].Casas[movimento.CasaObjetivo.y].Ocupante = null;
                    //}
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

                            if (subAdjacentes.Count(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario) == subAdjacentes.Count && cadaAdjacente.Ocupante.Value == TipoJogador.Rei)
                            {
                                message.Add(string.Format("O Rei foi encurralado em: ({0},{1})", cadaAdjacente.x, cadaAdjacente.y));
                                tabuleiro.Colunas[cadaAdjacente.x].Casas[cadaAdjacente.y].Ocupante = null;
                            }
                        }
                    }
                }

                return tabuleiro;
            }
            catch (Exception ex)
            {
                message = new List<string>();
                message.Add(ex.Message);
                return null;
            }
        }

        private VMTabuleiro OrganizarPecas(VMTabuleiro tabuleiro)
        {
            try
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
            catch (Exception ex)
            {
                return null;
            }
        }

        private VMTabuleiro GerarMatriz()
        {
            try
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
            catch (Exception ex)
            {
                return null;
            }
        }

        private string CodificarPecas(VMTabuleiro tabuleiro)
        {
            try
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
            catch (Exception ex)
            {
                return null;
            }
        }

        private VMTabuleiro DecodificarPecas(VMTabuleiro tabuleiro, string pecas)
        {
            try
            {

            }
            catch (Exception ex)
            {
                return null;
            }
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
            try
            {

            }
            catch (Exception ex)
            {
                return null;
            }
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
            try
            {
                message = "Objetivo concluído por: " + GetTipoJogadorString(tabuleiro.jogadorAtual);

                if (!tabuleiro.Colunas.Any(y => y.Casas.Any(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Rei)) && tabuleiro.jogadorAtual == TipoJogador.Mercenario)
                {
                    return true;
                }

                var objetivos = new List<VMCasaTabuleiro>() { tabuleiro.Colunas[0].Casas[0], tabuleiro.Colunas[10].Casas[0], tabuleiro.Colunas[0].Casas[10], tabuleiro.Colunas[10].Casas[10] };

                if (objetivos.Any(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Rei))
                {
                    return true;
                }

                message = "Objetivo ainda não concluído por: " + GetTipoJogadorString(tabuleiro.jogadorAtual);
                return false;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        private bool EstouEmPerigo(VMTabuleiro tabuleiro, VMCasaTabuleiro casa)
        {
            try
            {

                var adjacentes = GetCasasAdjacentes(tabuleiro, casa);

                var guardaReal = new List<TipoJogador>() { TipoJogador.Rei, TipoJogador.Soldado };

                var casasObjetivo = new List<VMCasaTabuleiro>() { tabuleiro.Colunas[0].Casas[0], tabuleiro.Colunas[0].Casas[10], tabuleiro.Colunas[10].Casas[0], tabuleiro.Colunas[10].Casas[10], tabuleiro.Colunas[5].Casas[5] };

                switch (tabuleiro.jogadorAtual)
                {
                    case TipoJogador.Rei:
                        if (adjacentes.Count(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario) > adjacentes.Count)
                        {
                            return true;
                        }

                        if (adjacentes.Count(x => x.Ocupante.HasValue && guardaReal.Contains(x.Ocupante.Value)) > 2 && adjacentes.Any(x => casasObjetivo.Contains(x)))
                        {
                            return true;
                        }
                        break;
                    case TipoJogador.Mercenario:
                        if (adjacentes.Count(x => x.Ocupante.HasValue && guardaReal.Contains(x.Ocupante.Value)) > 1)
                        {
                            return true;
                        }

                        if (adjacentes.Count(x => x.Ocupante.HasValue && guardaReal.Contains(x.Ocupante.Value)) > 0 && adjacentes.Any(x => casasObjetivo.Contains(x)))
                        {
                            return true;
                        }
                        break;
                    case TipoJogador.Soldado:
                        if (adjacentes.Count(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario) > 1)
                        {
                            return true;
                        }

                        if (adjacentes.Count(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario) > 0 && adjacentes.Any(x => casasObjetivo.Contains(x)))
                        {
                            return true;
                        }
                        break;
                    default:
                        break;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool EstouAjudandoACercar(VMTabuleiro tabuleiro, VMCasaTabuleiro casa)
        {
            try
            {

                var adjacentes = GetCasasAdjacentes(tabuleiro, casa);

                if (tabuleiro.jogadorAtual != TipoJogador.Mercenario)
                {
                    adjacentes = adjacentes.Where(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario).ToList();
                }
                else
                {
                    adjacentes = adjacentes.Where(x => x.Ocupante.HasValue && x.Ocupante.Value != TipoJogador.Mercenario).ToList();
                }

                foreach (var cadaAdjacente in adjacentes)
                {
                    var subAdjacentes = GetCasasAdjacentes(tabuleiro, cadaAdjacente);

                    if (subAdjacentes.Any(x => x.Ocupante.HasValue && x.Ocupante.Value == tabuleiro.jogadorAtual))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool PossoTocarORei(VMTabuleiro tabuleiro, VMMovimento movimento)
        {
            try
            {
                var rei = tabuleiro.Colunas.FirstOrDefault(z => z.Casas.Any(y => y.Ocupante.HasValue && y.Ocupante.Value == TipoJogador.Rei)).Casas.FirstOrDefault(y => y.Ocupante.HasValue && y.Ocupante.Value == TipoJogador.Rei);

                var adjacenteRei = GetCasasAdjacentes(tabuleiro, rei);

                return adjacenteRei.Contains(movimento.CasaObjetivo);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private VMTabuleiro SimularJogada(VMTabuleiro tabuleiro, VMMovimento movimento)
        {
            try
            {
                movimento.CasaAtual.Ocupante = tabuleiro.jogadorAtual;

                var tabuleiroSimulado = new VMTabuleiro(tabuleiro.Colunas, movimento.CasaAtual.Ocupante.Value);

                List<string> messages;
                tabuleiroSimulado = RealizarMovimento(tabuleiroSimulado, movimento, out messages);

                return tabuleiroSimulado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool PossoSalvarAlguem(VMTabuleiro tabuleiro, VMMovimento movimento)
        {
            try
            {

            }
            catch (Exception ex)
            {
            }

            return false;

            //if (tabuleiro.jogadorAtual == TipoJogador.Mercenario)
            //{
            //    var adjacentes = GetCasasAdjacentes(tabuleiro, movimento.CasaObjetivo);

            //    adjacentes = adjacentes.Where(x => x.Ocupante.HasValue && )
            //}
            //else
            //{
            //    var movimentos = 
            //}
        }

        private int CalcularDistancia(VMCasaTabuleiro casaOrigem, VMCasaTabuleiro casaDestino)
        {
            try
            {
                var distancia = (casaDestino.x - casaOrigem.x) * (casaDestino.x - casaOrigem.x);
                distancia += (casaDestino.y - casaOrigem.y) * (casaDestino.y - casaOrigem.y);

                return distancia;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        private VMCasaTabuleiro MelhorObjetivo(VMTabuleiro tabuleiro, TipoJogador jogadorAtual)
        {
            try
            {
                var rei = tabuleiro.Colunas.FirstOrDefault(z => z.Casas.Any(y => y.Ocupante.HasValue && y.Ocupante.Value == TipoJogador.Rei)).Casas.FirstOrDefault(y => y.Ocupante.HasValue && y.Ocupante.Value == TipoJogador.Rei);

                switch (jogadorAtual)
                {
                    case TipoJogador.Mercenario:
                        return rei;
                    case TipoJogador.Rei:
                        var objetivos = new List<VMCasaTabuleiro>() { tabuleiro.Colunas[0].Casas[0], tabuleiro.Colunas[10].Casas[0], tabuleiro.Colunas[0].Casas[10], tabuleiro.Colunas[10].Casas[10] };

                        return objetivos.OrderBy(x => CalcularDistancia(x, rei)).FirstOrDefault();
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<VMHeuristica> AbrirCaminho(VMTabuleiro tabuleiro)
        {
            try
            {

                if (tabuleiro.jogadorAtual == TipoJogador.Soldado)
                {
                    var rei = MelhorObjetivo(tabuleiro, TipoJogador.Mercenario);

                    var adjacentes = GetCasasAdjacentes(tabuleiro, rei);

                    var heuristica = new List<VMHeuristica>();

                    foreach (var cadaAdjacente in adjacentes)
                    {
                        var movimentos = GetMovimentosDisponiveisPorPeca(tabuleiro, cadaAdjacente);

                        if (movimentos.Any())
                        {
                            foreach (var cadaMovimento in movimentos)
                            {
                                heuristica.Add(new VMHeuristica(cadaMovimento, 0, EstouEmPerigo(tabuleiro, cadaMovimento.CasaObjetivo), false, false, 0));
                            }
                        }
                        else
                        {
                            var subAdjacentes = GetCasasAdjacentes(tabuleiro, cadaAdjacente);

                            foreach (var cadaSubAdjacentes in subAdjacentes)
                            {
                                var movimentosPorPeca = GetMovimentosDisponiveisPorPeca(tabuleiro, cadaSubAdjacentes);

                                if (movimentosPorPeca.Any())
                                {
                                    foreach (var cadaMovimento in movimentosPorPeca)
                                    {
                                        heuristica.Add(new VMHeuristica(cadaMovimento, 0, EstouEmPerigo(tabuleiro, cadaMovimento.CasaObjetivo), false, false, 0));
                                    }
                                }
                            }
                        }

                    }

                    return heuristica.OrderBy(x => x.Valor).ToList();
                }

                return new List<VMHeuristica>(); ;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}