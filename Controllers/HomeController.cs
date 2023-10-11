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

                ViewBag.autoClick = false;

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

                List<VMCasaTabuleiro> melhorCaminhoPercorrido;
                var melhorMovimento = MelhorMovimento(form.Tabuleiro, out melhorCaminhoPercorrido);

                if (melhorMovimento != null)
                {
                    if (melhorCaminhoPercorrido != null)
                    {
                        form.Tabuleiro = MostrarCaminhoRei(form.Tabuleiro, melhorCaminhoPercorrido);
                    }

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

        public ActionResult Proximo(string pecas, bool autoClick)
        {
            var form = new VMFormTabuleiro();

            ViewBag.autoClick = autoClick;

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
                    if (tabuleiro.Colunas[i].Casas[Peca.y].Ocupante.HasValue || tabuleiro.Colunas[i].Casas[Peca.y].EhObjetivo && tabuleiro.jogadorAtual != TipoJogador.Rei)
                    {
                        break;
                    }

                    //REVISAR A DISTANCIA
                    movimentos.Add(new VMMovimento(tabuleiro.Colunas[i].Casas[Peca.y], TipoMovimento.Vertical, Peca));
                }

                for (i = Peca.x - 1; i >= 0; i--)
                {
                    if (tabuleiro.Colunas[i].Casas[Peca.y].Ocupante.HasValue || tabuleiro.Colunas[i].Casas[Peca.y].EhObjetivo && tabuleiro.jogadorAtual != TipoJogador.Rei)
                    {
                        break;
                    }

                    //REVISAR A DISTANCIA
                    movimentos.Add(new VMMovimento(tabuleiro.Colunas[i].Casas[Peca.y], TipoMovimento.Vertical, Peca));
                }

                for (j = Peca.y + 1; j < 11; j++)
                {
                    if (tabuleiro.Colunas[Peca.x].Casas[j].Ocupante.HasValue || tabuleiro.Colunas[Peca.x].Casas[j].EhObjetivo && tabuleiro.jogadorAtual != TipoJogador.Rei)
                    {
                        break;
                    }

                    //REVISAR A DISTANCIA
                    movimentos.Add(new VMMovimento(tabuleiro.Colunas[Peca.x].Casas[j], TipoMovimento.Horizontal, Peca));
                }

                for (j = Peca.y - 1; j >= 0; j--)
                {
                    if (tabuleiro.Colunas[Peca.x].Casas[j].Ocupante.HasValue || tabuleiro.Colunas[Peca.x].Casas[j].EhObjetivo && tabuleiro.jogadorAtual != TipoJogador.Rei)
                    {
                        break;
                    }

                    //REVISAR A DISTANCIA
                    movimentos.Add(new VMMovimento(tabuleiro.Colunas[Peca.x].Casas[j], TipoMovimento.Horizontal, Peca));
                }

                return movimentos.Distinct().ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private VMMovimento MelhorMovimento(VMTabuleiro tabuleiro, out List<VMCasaTabuleiro> melhorCaminhoPercorrido)
        {
            try
            {
                var listaMovimentos = new List<VMHeuristica>();
                melhorCaminhoPercorrido = new List<VMCasaTabuleiro>();

                switch (tabuleiro.jogadorAtual)
                {
                    case TipoJogador.Soldado:
                        listaMovimentos = HeuristicaSoldado(tabuleiro, out melhorCaminhoPercorrido);
                        break;
                    case TipoJogador.Rei:
                        listaMovimentos = HeuristicaRei(tabuleiro);
                        break;
                    case TipoJogador.Mercenario:
                        listaMovimentos = HeuristicaMercenario(tabuleiro, out melhorCaminhoPercorrido);
                        break;
                }

                if (listaMovimentos == null || !listaMovimentos.Any())
                {
                    melhorCaminhoPercorrido = null;
                    return null;
                }

                int primeiraOpcao = listaMovimentos[0].Valor;

                listaMovimentos = listaMovimentos.Where(x => x.Valor == primeiraOpcao).ToList();

                var rand = new Random();

                var c = rand.Next(0, listaMovimentos.Count - 1);

                return listaMovimentos[c].Movimento;
            }
            catch (Exception ex)
            {
                melhorCaminhoPercorrido = null;
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
            catch (Exception ex)
            {
                return null;
            }
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
                return (int)(Math.Sqrt(Math.Pow(casaDestino.x - casaOrigem.x, 2) + Math.Pow(casaDestino.y - casaOrigem.y, 2)));
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        private List<VMCasaTabuleiro> CasasPercorridas(VMTabuleiro tabuleiro, VMMovimento movimento)
        {
            try
            {
                var lista = new List<VMCasaTabuleiro>();

                if (movimento.CasaAtual.x == movimento.CasaObjetivo.x)
                {
                    if (movimento.CasaAtual.y < movimento.CasaObjetivo.y)
                    {
                        for (int i = movimento.CasaAtual.y + 1; i <= movimento.CasaObjetivo.y; i++)
                        {
                            lista.Add(tabuleiro.Colunas[movimento.CasaAtual.x].Casas[i]);
                        }
                    }
                    else
                    {
                        for (int i = movimento.CasaAtual.y - 1; i >= movimento.CasaObjetivo.y; i--)
                        {
                            lista.Add(tabuleiro.Colunas[movimento.CasaAtual.x].Casas[i]);
                        }
                    }
                }
                else
                {
                    if (movimento.CasaAtual.x < movimento.CasaObjetivo.x)
                    {
                        for (int i = movimento.CasaAtual.x + 1; i <= movimento.CasaObjetivo.x; i++)
                        {
                            lista.Add(tabuleiro.Colunas[i].Casas[movimento.CasaObjetivo.y]);
                        }
                    }
                    else
                    {
                        for (int i = movimento.CasaAtual.x - 1; i >= movimento.CasaObjetivo.x; i--)
                        {
                            lista.Add(tabuleiro.Colunas[i].Casas[movimento.CasaObjetivo.y]);
                        }
                    }
                }

                return lista;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        private VMCasaTabuleiro MelhorObjetivo(VMTabuleiro tabuleiro, TipoJogador jogadorAtual)
        {
            try
            {
                var rei = new VMCasaTabuleiro();

                foreach (var cadaColuna in tabuleiro.Colunas)
                {
                    foreach (var cadaCasa in cadaColuna.Casas)
                    {
                        if (cadaCasa.Ocupante.HasValue && cadaCasa.Ocupante.Value == TipoJogador.Rei)
                        {
                            rei = cadaCasa;
                        }
                    }
                }

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

        private List<VMHeuristica> HeuristicaSoldado(VMTabuleiro tabuleiro, out List<VMCasaTabuleiro> caminhoPercorrido)
        {
            try
            {
                var listaCaminhoPercorrido = new List<VMCasaTabuleiro>(); ;

                if (tabuleiro.jogadorAtual == TipoJogador.Soldado)
                {
                    var rei = MelhorObjetivo(tabuleiro, TipoJogador.Mercenario);

                    var movimentosRei = GetMovimentosDisponiveisPorPeca(tabuleiro, rei);

                    var soldadosProximos = new List<VMCasaTabuleiro>();
                    var mercenariosProximos = new List<VMCasaTabuleiro>();

                    var adjacentes = GetCasasAdjacentes(tabuleiro, rei);
                    List<VMCasaTabuleiro> subAdjacentes = null;

                    foreach (var cadaAdjacente in adjacentes)
                    {
                        if (cadaAdjacente.Ocupante.HasValue && cadaAdjacente.Ocupante.Value == TipoJogador.Mercenario)
                        {
                            mercenariosProximos.Add(cadaAdjacente);
                        }
                        else
                        {
                            if (cadaAdjacente.Ocupante.HasValue && cadaAdjacente.Ocupante.Value == TipoJogador.Soldado)
                            {
                                soldadosProximos.Add(cadaAdjacente);
                            }
                        }

                        subAdjacentes = GetCasasAdjacentes(tabuleiro, cadaAdjacente);

                        mercenariosProximos.AddRange(subAdjacentes.Where((x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Mercenario)).ToList());

                        soldadosProximos.AddRange(subAdjacentes.Where((x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Soldado)).ToList());
                    }

                    mercenariosProximos = mercenariosProximos.Distinct().ToList();

                    soldadosProximos = soldadosProximos.Distinct().ToList();

                    var heuristica = new List<VMHeuristica>();

                    if (movimentosRei.Count > 1)
                    {
                        var movimentosPossiveis = GetMovimentosPossiveis(tabuleiro);

                        movimentosPossiveis = movimentosPossiveis.Where(x => !adjacentes.Contains(x.CasaObjetivo)).ToList();

                        if (mercenariosProximos.Count >= soldadosProximos.Count)
                        {
                            movimentosPossiveis = movimentosPossiveis.Where(x => !soldadosProximos.Contains(x.CasaAtual)).ToList();
                        }

                        var melhorObjetivo = MelhorObjetivo(tabuleiro, TipoJogador.Mercenario);

                        var tabuleiroSimulado = new VMTabuleiro(tabuleiro.Colunas, TipoJogador.Rei);

                        listaCaminhoPercorrido = CaminhoDoReiObjetivo(tabuleiroSimulado);

                        if (listaCaminhoPercorrido != null && listaCaminhoPercorrido.Any())
                        {
                            movimentosPossiveis = movimentosPossiveis.Where(x => !listaCaminhoPercorrido.Contains(x.CasaObjetivo)).ToList();
                        }

                        foreach (var cadaMovimento in movimentosPossiveis)
                        {
                            var cercando = EstouAjudandoACercar(tabuleiro, cadaMovimento.CasaObjetivo);
                            var estouEmPerigo = EstouEmPerigo(tabuleiro, cadaMovimento.CasaObjetivo);
                            var distanciaDoRei = CalcularDistancia(cadaMovimento.CasaAtual, rei);
                            var estouHaUmPasso = cadaMovimento.CasaAtual.x == rei.x || cadaMovimento.CasaAtual.y == rei.y;

                            heuristica.Add(new VMHeuristica(cadaMovimento, cercando, estouEmPerigo, distanciaDoRei, estouHaUmPasso));
                        }

                    }
                    else
                    {
                        //O REI NÃO TEM MOVIMENTO
                        soldadosProximos = adjacentes.Where(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Soldado).ToList();

                        foreach (var cadaSoldado in soldadosProximos)
                        {
                            //CADA SOLDADO PROXIMO DO REI
                            var movimentosSoldado = GetMovimentosDisponiveisPorPeca(tabuleiro, cadaSoldado);

                            if (movimentosSoldado != null && movimentosSoldado.Any())
                            {
                                //SOLDADO PODE SE MECHER
                                foreach (var cadaMovimento in movimentosSoldado)
                                {
                                    var cercando = EstouAjudandoACercar(tabuleiro, cadaMovimento.CasaObjetivo);
                                    var estouEmPerigo = EstouEmPerigo(tabuleiro, cadaMovimento.CasaObjetivo);
                                    var distanciaDoRei = CalcularDistancia(cadaMovimento.CasaAtual, rei);

                                    distanciaDoRei = distanciaDoRei * 2;

                                    heuristica.Add(new VMHeuristica(cadaMovimento, cercando, estouEmPerigo, distanciaDoRei, false));
                                }

                                break;
                            }
                            else
                            {
                                //BUSCAR ADJACENTES DOS SOLDADOS
                                var adjacentesSoldado = GetCasasAdjacentes(tabuleiro, cadaSoldado);

                                adjacentesSoldado = adjacentesSoldado.Where(x => x.Ocupante.HasValue && x.Ocupante.Value == TipoJogador.Soldado).ToList();

                                foreach (var cadaAdjacente in adjacentesSoldado)
                                {
                                    //CADA SOLDADO PROXIMO DO SOLDADO
                                    var movimentosAdjacente = GetMovimentosDisponiveisPorPeca(tabuleiro, cadaAdjacente);

                                    if (movimentosAdjacente != null && movimentosAdjacente.Any())
                                    {
                                        //SOLDADO PODE SE MECHER
                                        foreach (var cadaMovimento in movimentosAdjacente)
                                        {

                                            var cercando = EstouAjudandoACercar(tabuleiro, cadaMovimento.CasaObjetivo);
                                            var estouEmPerigo = EstouEmPerigo(tabuleiro, cadaMovimento.CasaObjetivo);
                                            var distanciaDoRei = CalcularDistancia(cadaMovimento.CasaAtual, rei);

                                            distanciaDoRei = distanciaDoRei * 2;

                                            heuristica.Add(new VMHeuristica(cadaMovimento, cercando, estouEmPerigo, distanciaDoRei, false));
                                        }
                                    }
                                }
                            }
                        }

                        //if (adjacentes.Any(x => !x.Ocupante.HasValue))
                        //{
                        //    var adjacenteComMovimento = adjacentes.FirstOrDefault(x => !x.Ocupante.HasValue);

                        //    var movimentosSoldado = GetMovimentosDisponiveisPorPeca(tabuleiro, adjacenteComMovimento);

                        //    foreach (var cadaMovimento in movimentosSoldado)
                        //    {
                        //        heuristica.Add(new VMHeuristica { Movimento = cadaMovimento, Valor = CalcularDistancia(cadaMovimento.CasaObjetivo, cadaMovimento.CasaAtual) });
                        //    }
                        //}
                        //else
                        //{


                        //    subAdjacentes = subAdjacentes.Distinct().ToList();

                        //    var subDaSub = new List<VMCasaTabuleiro>();

                        //    if (!subAdjacentes.Any(x => !x.Ocupante.HasValue))
                        //    {
                        //        foreach (var cadaAdjacente in subAdjacentes)
                        //        {
                        //            subDaSub.AddRange(GetCasasAdjacentes(tabuleiro, cadaAdjacente));
                        //        }

                        //        subDaSub = subDaSub.Distinct().ToList();

                        //        subAdjacentes = subDaSub;
                        //    }
                        //}

                        //var subAdjacentes = new List<VMCasaTabuleiro>();



                        //subAdjacentes = subAdjacentes.Where(x => !x.Ocupante.HasValue).ToList();

                        //var movimentosPossiveis = GetMovimentosPossiveis(tabuleiro);

                        //movimentosPossiveis = movimentosPossiveis.Where(x => !soldadosProximos.Contains(x.CasaAtual)).ToList();

                        //movimentosPossiveis = movimentosPossiveis.Where(x => subAdjacentes.Contains(x.CasaObjetivo)).ToList();

                        //foreach (var cadaMovimento in movimentosPossiveis)
                        //{
                        //    heuristica.Add(new VMHeuristica { Movimento = cadaMovimento, Valor = CalcularDistancia(cadaMovimento.CasaObjetivo, cadaMovimento.CasaAtual) });
                        //}
                    }

                    caminhoPercorrido = listaCaminhoPercorrido;
                    return heuristica.OrderBy(x => x.Valor).ToList();
                }

                caminhoPercorrido = null;
                return new List<VMHeuristica>();
            }
            catch (Exception ex)
            {
                caminhoPercorrido = null;
                return null;
            }
        }

        private List<VMHeuristica> HeuristicaMercenario(VMTabuleiro tabuleiro, out List<VMCasaTabuleiro> melhorCaminhoPercorrido)
        {
            try
            {
                var listaMovimentos = new List<VMHeuristica>();
                melhorCaminhoPercorrido = new List<VMCasaTabuleiro>();

                var movimentosPossiveis = GetMovimentosPossiveis(tabuleiro);

                var tabuleiroSimulado = new VMTabuleiro(tabuleiro.Colunas, TipoJogador.Rei);

                var listaCaminhoPercorrido = CaminhoDoReiObjetivo(tabuleiroSimulado);

                var melhorObjetivo = MelhorObjetivo(tabuleiroSimulado, TipoJogador.Mercenario);

                melhorCaminhoPercorrido = new List<VMCasaTabuleiro>();

                for (var z = 0; z < movimentosPossiveis.Count; z++)
                {
                    var perigo = EstouEmPerigo(tabuleiro, movimentosPossiveis[z].CasaObjetivo);
                    var cercando = EstouAjudandoACercar(tabuleiro, movimentosPossiveis[z].CasaObjetivo);
                    var tocandoRei = PossoTocarORei(tabuleiro, movimentosPossiveis[z]);
                    var distanciaReiObjetivo = 0;
                    var vouPassarPorLa = false;

                    if (listaCaminhoPercorrido != null && listaCaminhoPercorrido.Any())
                    {
                        vouPassarPorLa = listaCaminhoPercorrido.Contains(movimentosPossiveis[z].CasaObjetivo);
                        //distanciaReiObjetivo = listaCaminhoPercorrido.Count;
                    }

                    var heuristica = new VMHeuristica(movimentosPossiveis[z], perigo, cercando, tocandoRei, vouPassarPorLa, distanciaReiObjetivo);

                    //if (listaMovimentos.Any() && listaMovimentos[0].Valor < heuristica.Valor)
                    //{
                    //    melhorCaminhoPercorrido = listaCaminhoPercorrido;
                    //}
                    //else if (!listaMovimentos.Any())
                    //{
                    //    melhorCaminhoPercorrido = listaCaminhoPercorrido;
                    //}

                    listaMovimentos.Add(heuristica);

                    listaMovimentos = listaMovimentos.OrderByDescending(y => y.Valor).ToList();
                }

                listaMovimentos = listaMovimentos.OrderByDescending(y => y.Valor).ToList();

                return listaMovimentos;
            }
            catch (Exception ex)
            {
                melhorCaminhoPercorrido = null;
                return null;
            }
        }

        private List<VMHeuristica> HeuristicaRei(VMTabuleiro tabuleiro)
        {
            try
            {
                var listaMovimentos = new List<VMHeuristica>();

                var movimentosPossiveis = GetMovimentosPossiveis(tabuleiro);

                var melhorObjetivo = MelhorObjetivo(tabuleiro, TipoJogador.Rei);

                for (var z = 0; z < movimentosPossiveis.Count; z++)
                {
                    var distanciaPercorrida = CalcularDistancia(melhorObjetivo, movimentosPossiveis[z].CasaObjetivo);
                    var estouEmPerigo = EstouEmPerigo(tabuleiro, movimentosPossiveis[z].CasaObjetivo);
                    var distObjetivo = CalcularDistancia(movimentosPossiveis[z].CasaObjetivo, melhorObjetivo);

                    listaMovimentos.Add(new VMHeuristica(movimentosPossiveis[z], distanciaPercorrida, estouEmPerigo, distObjetivo));
                }

                listaMovimentos = listaMovimentos.OrderBy(y => y.Valor).ToList();

                return listaMovimentos;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool PossoInterceptarRei(VMTabuleiro tabuleiro, VMMovimento movimentoRei)
        {
            var reiPassaraAqui = CasasPercorridas(tabuleiro, movimentoRei);

            return reiPassaraAqui.Contains(movimentoRei.CasaObjetivo);
        }

        private List<VMCasaTabuleiro> CaminhoDoReiObjetivo(VMTabuleiro tabuleiroSimulado)
        {
            try
            {
                tabuleiroSimulado.jogadorAtual = TipoJogador.Rei;
                var listaCaminhoPercorrido = new List<VMCasaTabuleiro>();

                int i = 0;
                var distanciaReiObjetivo = 1;


                while (distanciaReiObjetivo > 0)
                {
                    var melhorObjetivo = MelhorObjetivo(tabuleiroSimulado, TipoJogador.Rei);

                    var movimentoHeuristicaRei = HeuristicaRei(tabuleiroSimulado);

                    if (movimentoHeuristicaRei == null || !movimentoHeuristicaRei.Any())
                    {
                        return null;
                    }

                    if (listaCaminhoPercorrido.Any(x => x.x == movimentoHeuristicaRei[0].Movimento.CasaObjetivo.x && x.y == movimentoHeuristicaRei[0].Movimento.CasaObjetivo.y))
                    {
                        return null;
                    }

                    var casasDoMovimento = CasasPercorridas(tabuleiroSimulado, movimentoHeuristicaRei[0].Movimento);

                    listaCaminhoPercorrido.AddRange(casasDoMovimento);

                    distanciaReiObjetivo = CalcularDistancia(movimentoHeuristicaRei[0].Movimento.CasaObjetivo, melhorObjetivo);

                    if (distanciaReiObjetivo > 0)
                    {
                        tabuleiroSimulado = SimularJogada(tabuleiroSimulado, movimentoHeuristicaRei[0].Movimento);
                        tabuleiroSimulado.jogadorAtual = TipoJogador.Rei;
                    }

                    listaCaminhoPercorrido = listaCaminhoPercorrido.Distinct().ToList();

                    i++;

                    if (i > 100)
                    {
                        return null;
                    }
                }

                return listaCaminhoPercorrido;
            }
            catch (Exception ex)
            {
                return new List<VMCasaTabuleiro>();
            }
        }

        private VMTabuleiro MostrarCaminhoRei(VMTabuleiro tabuleiro, List<VMCasaTabuleiro> casas)
        {
            foreach (var cadaCasa in casas)
            {
                tabuleiro.Colunas[cadaCasa.x].Casas[cadaCasa.y].movida = true;
            }

            return tabuleiro;
        }
    }
}