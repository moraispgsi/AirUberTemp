using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirUberProjeto.Data;
using AirUberProjeto.Models;
using AirUberProjeto.Models.CompanhiaViewModels;
using AirUberProjeto.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AirUberProjeto.Controllers
{
    /// <summary>
    /// Classe respons�vel por receber todos os pedidos do browser e tratar dos mesmos relativamente �s companhias
    /// </summary>
    public class CompanhiaController : Controller
    {

        /// <summary>
        /// Utilizado para saber o caminho absoluto da pasta wwwRoot
        /// </summary>
        private readonly IHostingEnvironment _environment;

        /// <summary>
        /// O contexto da aplica��o para poder aceder a dados.
        /// </summary>
        private readonly AirUberDbContext _context;

        /// <summary>
        /// User manager que vai permitir utilizar metodos feitos pela windows de forma a controlar os user.
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;
        /// <summary>
        /// Server para enviar os emails.
        /// </summary>
        private readonly IEmailSender _emailSender;



        /// <summary>
        /// Construtor do controlador Companhia
        /// </summary>
        /// <param name="context">O contexto da aplica��o</param>
        /// <param name="userManager">O manager dos utilizadores</param>
        /// <param name="environment">O ambiente da aplica��o</param>
        /// <param name="emailSender">O email sender a usar para enviar os emails</param>
        public CompanhiaController(AirUberDbContext context, UserManager<ApplicationUser> userManager, 
                                   IHostingEnvironment environment, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Redirecciona para a ac��o 'Perfil Companhia'
        /// </summary>
        /// <returns>Retorna a View retornada pela ac��o Perfil Companhia</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("PerfilCompanhia");
        }

        /// <summary>
        /// Apresenta a p�gina de perfil da companhia
        /// </summary>
        /// <returns>A View de perfil da companhia</returns>
        [HttpGet]
        public IActionResult PerfilCompanhia()
        {


            Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;


            Companhia companhia = (_context.Companhia.Select(c => c).Include(c => c.Pais)
                                                             .Include(c => c.Estado)
                                                             .Include(c => c.ContaDeCreditos)
                                                             .Include(c => c.ListaReservas)
                                                             .Include(c => c.ListaColaboradores)
                                                             .Include(c => c.ListaJatos)
                                                             .Include(c => c.ListaExtras)
                                                             .Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

            PerfilCompanhiaViewModel perfilViewModel = new PerfilCompanhiaViewModel()
            {
                Colaborador = colaborador,
                Companhia = companhia,
            };


            List<Notificacao> notificacoes = _context.Notificacao.Where(n => n.UtilizadorId == colaborador.Id).ToList();

            foreach (Notificacao notificacao in notificacoes)
            {
                perfilViewModel.Notificacoes.Add(notificacao);

            }

            return View(perfilViewModel);
        }

        /// <summary>
        /// Apresenta a p�gina de edi��o de perfil da companhia
        /// </summary>
        /// <returns>A View de edi��o do perfil da companhia</returns>
        [HttpGet]
        public IActionResult EditarPerfilCompanhia()
        {
            Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;

            Companhia companhia = (_context.Companhia.Select(c => c).Include(c => c.Pais)
                                                             .Include(c => c.Estado)
                                                             .Include(c => c.ContaDeCreditos)
                                                             .Include(c => c.ListaReservas)
                                                             .Include(c => c.ListaColaboradores)
                                                             .Include(c => c.ListaJatos)
                                                             .Include(c => c.ListaExtras)
                                                             .Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();
            return View(companhia);
        }


        /// <summary>
        /// Trata de um pedido de altera��o de dados de uma companhia
        /// </summary>
        /// <param name="viewModel">ViewModel do pedido de altera��o de dados</param>
        /// <returns>A view de edi��o de perfil da companhia</returns>
        [HttpPost]
        public IActionResult EditarPerfilCompanhia(EditarCompanhiaViewModel viewModel)
        {
           
            if (ModelState.IsValid) // se os dados forem v�lidos
            {
                Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;
                Companhia companhia = (_context.Companhia.Select(c => c).Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

                companhia.Nome = viewModel.Nome;
                companhia.Nif = viewModel.Nif;
                companhia.Contact = viewModel.Contact;
                _context.Update(companhia);
                _context.SaveChanges();
                ViewData["Success"] = true;
                return View(companhia);
            }
            
            return RedirectToAction("EditarPerfilCompanhia");
        }


        /***********************
         *       Jatos         *
         *                     *
         ***********************/

        /// <summary>
        /// Apresenta a p�gina com todos os jatos da companhia 
        /// </summary>
        /// <returns>View para visualizar jatos</returns>
        [HttpGet]
        public IActionResult VerJatos()
        {
            Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;
            Companhia companhia = (_context.Companhia.Select(c => c).Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

            var jatos = _context.Jato.Select(j => j).Include(j => j.Modelo)
                                                    .Include(j => j.Modelo.TipoJato)
                                                    .Include(j => j.Companhia)
                                                    .Include(j => j.Aeroporto)
                                                    .Where(j => j.CompanhiaId == companhia.CompanhiaId);
            return View(jatos);
        }


        /// <summary>
        /// Apresenta a p�gina para editar os dados dos jatos
        /// </summary>
        /// <param name="id">identificador �nico do jato</param>
        /// <returns>View para visualizar a p�gina de edi��o de jatos</returns>
        [HttpGet]
        public IActionResult EditarJatos(int? id)
        {

            if(id != null) //se o id do jato existe, ou seja, se foi selecionado um jato
            {
                Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;

                Companhia companhia = (_context.Companhia.Select(c => c).Include(c => c.Pais)
                                                                 .Include(c => c.Estado)
                                                                 .Include(c => c.ContaDeCreditos)
                                                                 .Include(c => c.ListaReservas)
                                                                 .Include(c => c.ListaColaboradores)
                                                                 .Include(c => c.ListaJatos)
                                                                 .Include(c => c.ListaExtras)
                                                                 .Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

                Jato jato = (_context.Jato.Select(c => c).Include(c => c.Aeroporto)
                                                         .Include(c => c.Companhia)
                                                         .Include(c => c.Modelo)
                                                         .Where(c => c.JatoId == id)).Single();

                var aeroportos = _context.Aeroporto.Select(a => new { Id = a.AeroportoId, Valor = a.Nome });

                var modelos = (from Modelo in _context.Modelo
                               join TipoJato in _context.TipoJato
                                   on Modelo.TipoJatoId equals TipoJato.TipoJatoId
                               select new { Modelo.ModeloId, TipoJato.Nome });

                ViewBag.aeroportos = new SelectList(aeroportos, "Id", "Valor");
                ViewBag.companhia = companhia.Nome;
                ViewBag.modelos = new SelectList(modelos, "ModeloId", "Nome");

                return View(jato);
            }
           
            return NotFound();
        }


        /// <summary>
        /// Trata de um pedido de altera��o de dados de um jato
        /// </summary>
        /// <param name="viewModel">ViewModel do pedido de altera��o de dados</param>
        /// <returns>A view de edi��o de um jato da companhia</returns>
        [HttpPost]
        public IActionResult EditarJatos(EditarJatoViewModel viewModel)
        {
            if (ModelState.IsValid) // se os dados forem v�lidos
            {
                Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;
                Companhia companhia = (_context.Companhia.Select(c => c).Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

                Jato jato = (_context.Jato.Select(c => c).Include(c => c.Aeroporto)
                                                         .Include(c => c.Companhia)
                                                         .Include(c => c.Modelo)
                                                         .Where(c => c.JatoId == viewModel.JatoId))
                                                         .Single();

                jato.Nome = viewModel.Nome;
                jato.AeroportoId = viewModel.AeroportoId;
                jato.ModeloId = viewModel.ModeloId;
                
                _context.Update(jato);
                _context.SaveChanges();
                return RedirectToAction("VerJatos");
            }

            return RedirectToAction("EditarJatos");
        }

        /// <summary>
        /// Apresenta a p�gina para criar jatos de uma companhia
        /// </summary>
        /// <returns>View para criar jatos</returns>
        [HttpGet]
        public IActionResult CriarJato()
        {

            Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;
            Companhia companhia = (_context.Companhia.Select(c => c).Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

            var aeroportos = _context.Aeroporto.Select(a => new { Id = a.AeroportoId, Valor = a.Nome });

            //var modelos = _context.Modelo.Select(m => new { Id = m.ModeloId, Valor = m.Descricao });

            // o nome do modelo esta no tipo de jato e � necess�rio o join, porque est�o em tabelas separadas
            var modelos = (from Modelo in _context.Modelo
                         join TipoJato in _context.TipoJato
                             on Modelo.TipoJatoId equals TipoJato.TipoJatoId
                         select new { Modelo.ModeloId, TipoJato.Nome });

            ViewBag.aeroportos = new SelectList(aeroportos, "Id", "Valor");
            ViewBag.companhia = companhia.Nome;
            ViewBag.modelos = new SelectList(modelos, "ModeloId", "Nome");

            return View();
        }


        /// <summary>
        /// Trata de um pedido de cria��o de um jato
        /// </summary>
        /// <param name="viewModel">ViewModel do pedido de cria��o de dados</param>
        /// <returns>A view de cria��o de um jato</returns>
        [HttpPost]
        public IActionResult CriarJato(CriarJatoViewModel viewModel)
        {
            if (ModelState.IsValid) // se os dados forem v�lidos
            {
                Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;

                Companhia companhia = (_context.Companhia.Select(c => c).Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

                //logger
                addAcaoColaborador(new Acao()
                {
                    TipoAcaoId = 3,  // insert
                    Target = "Criar Jato",
                    Detalhes = "O colaborador " + colaborador.Nome + " " + colaborador.Apelido + ", que pertence " +
                                "� companhia de nome " + companhia.Nome + " tentou criar um jato",

                }, colaborador);

                Jato jato = new Jato()
                {
                    Companhia = companhia,
                    Nome = viewModel.Nome,
                    AeroportoId = viewModel.AeroportoId,
                    CompanhiaId = viewModel.CompanhiaId,
                    EmFuncionamento = false,
                    ModeloId = viewModel.ModeloId,
                };

                companhia.ListaJatos.Add(jato);
                _context.Update(companhia);
                _context.SaveChanges();
                ViewData["Success"] = true;
                return RedirectToAction("VerJatos");
            }

            return RedirectToAction("CriarJato");
        }


        [HttpGet]
        public IActionResult ApagarJato (int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jato = _context.Jato.Select(j => j)
                                    .Include(j => j.Aeroporto)
                                    .Include(j => j.Companhia)
                                    .Include(j => j.Modelo)
                                    .Include(j => j.Modelo.TipoJato)
                                    .SingleOrDefault(j => j.JatoId == id);
            if (jato == null)
            {
                return NotFound();
            }
            return View(jato);
        }

        [HttpPost, ActionName("ApagarJato")]
        [ValidateAntiForgeryToken]
        public IActionResult ApagarJatoConfirmacao (int? id)
        {

            var jato = _context.Jato.SingleOrDefault(j => j.JatoId == id);

            _context.Jato.Remove(jato);
            _context.SaveChanges();

            return RedirectToAction("VerJatos");
        }


        /***********************
         *    Colaboradores    *
         *                     *
         ***********************/

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult VerColaboradores()
        {
            Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;
            Companhia companhia = (_context.Companhia.Select(c => c)
                                                     .Include(c => c.ListaColaboradores)
                                                     .Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

            return View(companhia.ListaColaboradores);
        }

        [HttpGet]
        public IActionResult AdicionarColaborador()
        {
            Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;

            ViewData["CompanhiaId"] = new SelectList(_context.Companhia.Select(c => c)
                                                                       .Where(c => c.CompanhiaId == colaborador.CompanhiaId), 
                                                                       "CompanhiaId", "Nome");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarColaborador(CriarColaboradorViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;

                Companhia companhia = (_context.Companhia.Select(c => c).Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();
                
                //logger
                addAcaoColaborador(new Acao()
                {
                    TipoAcaoId = 3,  // insert
                    Target = "Criar Colaborador",
                    Detalhes = "O colaborador " + colaborador.Nome + " " + colaborador.Apelido + ", que pertence " +
                                "� companhia de nome " + companhia.Nome + " tentou criar um colaborador",

                }, colaborador);
                

                Colaborador novoColaborador = new Colaborador()
                {
                    Nome = viewModel.PrimeiroNome,
                    Apelido = viewModel.Apelido,
                    Email = viewModel.Email,
                    //CompanhiaId = viewModel.CompanhiaId,  // Se usar o ID � apresentado um erro!
                    Companhia = companhia,
                    IsAdministrador = viewModel.IsAdministrador,
                    UserName = viewModel.Email,
                };

                //criar utilizador colaborador, para se poder autenticar
                var result = await _userManager.CreateAsync(novoColaborador, viewModel.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(novoColaborador, novoColaborador.IsAdministrador ? Roles.ROLE_COLABORADOR_ADMIN : Roles.ROLE_COLABORADOR);//atribui a role


                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(novoColaborador);
                    var callbackUrl = Url.Action("ConfirmEmail", "Autenticacao", new { userId = novoColaborador.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    await _emailSender.SendEmailAsync(novoColaborador.Email, "Confirm your account",
                        $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                    //await _signInManager.SignInAsync(novoColaborador, isPersistent: false);//para ele depois fazer login, regista-se e fica logo loged-in
                    //_logger.LogInformation(3, "User created a new account with password.");
                }



                companhia.ListaColaboradores.Add(novoColaborador);

                _context.Update(companhia);
                _context.SaveChanges();
                return RedirectToAction("VerColaboradores");
            }

            return RedirectToAction("AdicionarColaborador");
        }

        /// <summary>
        /// Apresenta a p�gina para editar os dados de um colaborador
        /// </summary>
        /// <param name="id">identificador �nico de um colaborador</param>
        /// <returns>View para visualizar a p�gina de edi��o de jatos</returns>
        [HttpGet]
        public IActionResult EditarColaborador(string id)
        {
            if(id != null)
            {
                Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;

                Companhia companhia = (_context.Companhia.Select(c => c).Include(c => c.Pais)
                                                                 .Include(c => c.Estado)
                                                                 .Include(c => c.ContaDeCreditos)
                                                                 .Include(c => c.ListaReservas)
                                                                 .Include(c => c.ListaColaboradores)
                                                                 .Include(c => c.ListaJatos)
                                                                 .Include(c => c.ListaExtras)
                                                                 .Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

                Colaborador colaboradorAEditar = (_context.Colaborador.Select(c => c)
                                                               .Include(c => c.Companhia)
                                                               .Where(c => c.Id == id)).Single();

                //logger
                addAcaoColaborador(new Acao()
                {
                    TipoAcaoId = 3,  // insert
                    Target = "Editar Colaborador",
                    Detalhes = "O colaborador " + colaborador.Nome + " " + colaborador.Apelido + ", que pertence " +
                                "� companhia de nome " + companhia.Nome + " tentou editar o colaborador " + colaboradorAEditar.Nome + " " + colaboradorAEditar.Apelido,

                }, colaborador);

                ViewBag.companhia = companhia.Nome;

                return View(colaboradorAEditar);
            }
            return NotFound();
        }


        [HttpPost]
        public IActionResult EditarColaborador(EditarColaboradorViewModel viewModel)
        {
            if (ModelState.IsValid) // se os dados forem v�lidos
            {
                Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;
                Companhia companhia = (_context.Companhia.Select(c => c).Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

                Colaborador colaboradorAEditar = (_context.Colaborador.Select(c => c)
                                                                .Include(c => c.Companhia)
                                                                .Where(c => c.Id == viewModel.Id)).Single();

                colaboradorAEditar.Nome = viewModel.Nome;
                colaboradorAEditar.Apelido = viewModel.Apelido;
                colaboradorAEditar.Email = viewModel.Email;
                colaboradorAEditar.IsAdministrador = viewModel.IsAdministrador;
                

                /*
                 * Ac��o - logger
                 * */
                _context.Update(colaboradorAEditar);
                _context.SaveChanges();
                return RedirectToAction("VerColaboradores");
            }

            return RedirectToAction("EditarColaborador");

        }


        [HttpGet]
        public IActionResult ApagarColaborador(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var colaborador = _context.Colaborador.Select(c => c)
                                      .Include(c => c.Companhia)
                                      .SingleOrDefault(c => c.Id == id);
            if (colaborador == null)
            {
                return NotFound();
            }
            return View(colaborador);
        }

        [HttpPost, ActionName("ApagarColaborador")]
        [ValidateAntiForgeryToken]
        public IActionResult ApagarColaboradorConfirmacao(string id)
        {

            var colaborador = _context.Colaborador.SingleOrDefault(j => j.Id == id);

            _context.Colaborador.Remove(colaborador);
            _context.SaveChanges();

            return RedirectToAction("VerColaboradores");
        }


        /***********************
         *       Extras        *
         *                     *
         ***********************/

        [HttpGet] 
        public IActionResult VerExtras()
        {

            Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;

            Companhia companhia = (_context.Companhia.Select(c => c)
                                                     .Include(c => c.ListaExtras)
                                                     .Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

            var extras = _context.Extra.Select(j => j)
                                      .Include(j => j.Companhia)
                                      .Include(j => j.TipoExtra)
                                      .Where(j => j.CompanhiaId == companhia.CompanhiaId);
        
            return View(extras);
        }

        [HttpGet]
        public IActionResult CriarExtra()
        {
            Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;

            Companhia companhia = (_context.Companhia.Select(c => c)
                                                     .Include(c => c.ListaExtras)
                                                     .Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();


            ViewData["CompanhiaId"] = new SelectList(_context.Companhia.Select(c => c).Where(c => c.CompanhiaId == companhia.CompanhiaId), "CompanhiaId", "Nome");
            ViewData["TipoExtraId"] = new SelectList(_context.TipoExtra, "TipoExtraId", "Nome");
            return View();
        }

        /*
         * 
         * 
         * 
         * 
         * Problema com valores decimais com '.'
         * 
         * 
         * 
         * 
         * 
         * 
         */ 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CriarExtra(CriarExtraViewModel viewModel)
        {
           
            if (ModelState.IsValid)
            {

                // � recebido o objecto da classe view model
                Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;

                Companhia companhia = (_context.Companhia.Select(c => c).Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();


                Extra extra = new Extra()
                {
                    CompanhiaId = viewModel.CompanhiaId,
                    Nome = viewModel.Nome,
                    TipoExtraId = viewModel.TipoExtraId,
                    Valor = Convert.ToDecimal(viewModel.Valor)
                };


                companhia.ListaExtras.Add(extra);
                _context.Update(companhia);
                _context.SaveChanges();

                return RedirectToAction("VerExtras");
            }
            ViewData["CompanhiaId"] = new SelectList(_context.Companhia, "CompanhiaId", "Contact", viewModel.CompanhiaId);
            ViewData["TipoExtraId"] = new SelectList(_context.TipoExtra, "TipoExtraId", "Nome", viewModel.TipoExtraId);
            return RedirectToAction("CriarExtra");
        }


        [HttpGet]
        public IActionResult EditarExtra(int id)
        {
            var extra = _context.Extra.Single(e => e.ExtraId == id);
            if(extra != null)
            {

                Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;

                Companhia companhia = (_context.Companhia.Select(c => c)
                                                         .Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

                ViewBag.companhia = companhia.Nome;
                ViewBag.tipos = new SelectList(_context.TipoExtra, "TipoExtraId", "Nome");
               
                return View(extra);
            }

            return NotFound();
        }
           
        [HttpPost]
        public IActionResult EditarExtra(EditarExtraViewModel viewModel)
        {
            if (ModelState.IsValid) // se os dados forem v�lidos
            {
                //Colaborador colaborador = (Colaborador)_userManager.GetUserAsync(this.User).Result;
                //Companhia companhia = (_context.Companhia.Select(c => c).Where(c => c.CompanhiaId == colaborador.CompanhiaId)).Single();

                Extra extraAEditar = (_context.Extra.Select(c => c).Where(c => c.ExtraId == viewModel.ExtraId)).Single();

                extraAEditar.Nome = viewModel.Nome;
                extraAEditar.TipoExtraId = viewModel.TipoExtraId;
                extraAEditar.Valor = viewModel.Valor;
               
                /*
                 * Ac��o - logger
                 * */

                _context.Update(extraAEditar);
                _context.SaveChanges();
                return RedirectToAction("VerExtras");
            }

            return RedirectToAction("EditarExtra");

        }


        [HttpGet]
        public IActionResult ApagarExtra(int id)
        {

            var extra = _context.Extra.Include(e => e.Companhia)
                                      .Include(e => e.TipoExtra)
                                      .Single(e => e.ExtraId == id);

            if (extra == null)
            {
                return NotFound();
            }

            ViewData["TipoExtraId"] = new SelectList(_context.TipoExtra, "TipoExtraId", "Nome");
            return View(extra);
        }

        [HttpPost, ActionName("ApagarExtra")]
        [ValidateAntiForgeryToken]
        public IActionResult ApagarExtraConfirmacao(int id)
        {

            var extra = _context.Extra.Single(e => e.ExtraId == id);

            _context.Extra.Remove(extra);
            _context.SaveChanges();

            return RedirectToAction("VerExtras");
        }


        /***********************
         *       Modelos       *
         *                     *
         ***********************/

        [HttpGet]
        public IActionResult VerModelos()
        {
            var modelos = _context.Modelo.Select(m => m)
                                         .Include(m => m.TipoJato);


            return View(modelos);
        }

        [HttpGet]
        public IActionResult AdicionarModelo()
        {

            ViewBag.tipos = new SelectList(_context.TipoJato, "TipoJatoId", "Nome");
            return View();
        }

        [HttpPost]
        public IActionResult AdicionarModelo(CriarModeloViewModel viewModel)
        {
            if (ModelState.IsValid)
            {

                Modelo modelo = new Modelo()
                {
                    Capacidade = viewModel.Capacidade,
                    Alcance = viewModel.Alcance,
                    VelocidadeMaxima = viewModel.VelocidadeMaxima,
                    PesoMaximaBagagens = viewModel.PesoMaximaBagagens,
                    NumeroMotores = viewModel.NumeroMotores,
                    AltitudeIdeal = viewModel.AltitudeIdeal,
                    AlturaCabine = viewModel.AlturaCabine,
                    LarguraCabine = viewModel.LarguraCabine,
                    ComprimentoCabine = viewModel.ComprimentoCabine,
                    Descricao = viewModel.Descricao,
                    TipoJatoId = viewModel.TipoJatoId,
                    
                };


                _context.Add(modelo);
                _context.SaveChanges();

                return RedirectToAction("VerModelos");
            }
            ViewBag.tipos = new SelectList(_context.TipoJato, "TipoJatoId", "Nome");
            return RedirectToAction("AdicionarModelo");
        }




        private void addAcaoColaborador (Acao acao, Colaborador colaborador)
        {
            colaborador.ListaAcoes.Add(acao);
        }

    }
}