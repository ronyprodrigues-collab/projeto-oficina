using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using Services;

namespace projetos.Controllers
{
    [Authorize]
    public class OficinasController : Controller
    {
        private readonly OficinaDbContext _db;
        private readonly IOficinaContext _oficinaContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public OficinasController(OficinaDbContext db, IOficinaContext oficinaContext, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _oficinaContext = oficinaContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Selecionar(string? returnUrl = null)
        {
            var viewModel = new SelecionarOficinaViewModel
            {
                ReturnUrl = returnUrl
            };

            if (User.IsInRole("SuporteTecnico"))
            {
                var grupos = await _db.Grupos
                    .Include(g => g.Oficinas)
                    .OrderBy(g => g.Nome)
                    .ToListAsync();

                viewModel.Grupos = grupos
                    .Select(g =>
                    {
                        var limite = ObterLimitePlano(g.Plano);
                        return new GrupoOficinaDisponivelViewModel
                        {
                            GrupoId = g.Id,
                            GrupoNome = g.Nome,
                            Plano = ObterPlanoLabel(g.Plano),
                            OficinasUsadas = g.Oficinas.Count,
                            OficinasPermitidas = limite,
                            PodeCriarNovas = !limite.HasValue || g.Oficinas.Count < limite.Value,
                            Oficinas = g.Oficinas
                                .OrderBy(o => o.Nome)
                                .Select(o => new OficinaDisponivelViewModel
                                {
                                    OficinaId = o.Id,
                                    Nome = o.Nome
                                }).ToList()
                        };
                    })
                    .ToList();
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var oficinaIds = await _db.OficinasUsuarios
                    .Where(ou => ou.UsuarioId == userId && ou.Ativo)
                    .Select(ou => ou.OficinaId)
                    .Distinct()
                    .ToListAsync();

                if (oficinaIds.Any())
                {
                    var grupos = await _db.Grupos
                        .Include(g => g.Oficinas)
                        .Where(g => g.Oficinas.Any(o => oficinaIds.Contains(o.Id)))
                        .OrderBy(g => g.Nome)
                        .ToListAsync();

                    viewModel.Grupos = grupos
                        .Select(g =>
                        {
                            var limite = ObterLimitePlano(g.Plano);
                            var oficinasDoUsuario = g.Oficinas
                                .Where(o => oficinaIds.Contains(o.Id))
                                .OrderBy(o => o.Nome)
                                .Select(o => new OficinaDisponivelViewModel
                                {
                                    OficinaId = o.Id,
                                    Nome = o.Nome
                                }).ToList();

                            return new GrupoOficinaDisponivelViewModel
                            {
                                GrupoId = g.Id,
                                GrupoNome = g.Nome,
                                Plano = ObterPlanoLabel(g.Plano),
                                OficinasUsadas = oficinasDoUsuario.Count,
                                OficinasPermitidas = limite,
                                PodeCriarNovas = !limite.HasValue || g.Oficinas.Count < limite.Value,
                                Oficinas = oficinasDoUsuario
                            };
                        })
                        .Where(vm => vm.Oficinas.Any())
                        .ToList();
                }
            }

            if (!viewModel.Grupos.Any())
            {
                TempData["Error"] = "Nenhuma oficina atribuída ao seu usuário. Solicite acesso ao administrador.";
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Selecionar(SelecionarOficinaViewModel model)
        {
            if (model.OficinaSelecionada <= 0)
            {
                TempData["Error"] = "Selecione uma oficina.";
                return RedirectToAction(nameof(Selecionar), new { returnUrl = model.ReturnUrl });
            }

            if (!await _oficinaContext.SetOficinaAtualAsync(model.OficinaSelecionada))
            {
                TempData["Error"] = "Não foi possível selecionar esta oficina.";
                return RedirectToAction(nameof(Selecionar), new { returnUrl = model.ReturnUrl });
            }

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Painel");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Limpar()
        {
            _oficinaContext.Clear();
            return RedirectToAction(nameof(Selecionar));
        }

        private static int? ObterLimitePlano(PlanoConta plano) => plano switch
        {
            PlanoConta.Basico => 1,
            PlanoConta.Pro => 2,
            PlanoConta.Plus => null,
            _ => null
        };

        private static string ObterPlanoLabel(PlanoConta plano) => plano switch
        {
            PlanoConta.Basico => "Básico",
            PlanoConta.Pro => "Pro",
            PlanoConta.Plus => "Plus",
            _ => plano.ToString()
        };

        [Authorize(Roles = "Admin,SuporteTecnico,Diretor")]
        public async Task<IActionResult> Nova(int? grupoId)
        {
            var vm = new NovaOficinaViewModel
            {
                Plano = PlanoConta.Basico,
                GrupoId = grupoId ?? 0
            };
            await PopularDadosNovaOficinaAsync(vm);
            return View(vm);
        }

        [Authorize(Roles = "Admin,SuporteTecnico,Diretor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nova(NovaOficinaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopularDadosNovaOficinaAsync(model);
                return View(model);
            }

            var grupo = await ObterGrupoPermitidoAsync(model.GrupoId);
            if (grupo == null)
            {
                ModelState.AddModelError(nameof(model.GrupoId), "Selecione um grupo válido.");
                await PopularDadosNovaOficinaAsync(model);
                return View(model);
            }

            var limite = ObterLimitePlano(grupo.Plano);
            if (limite.HasValue && grupo.Oficinas.Count >= limite.Value)
            {
                ModelState.AddModelError(string.Empty, $"Plano {grupo.Plano} permite no máximo {limite.Value} oficinas.");
                await PopularDadosNovaOficinaAsync(model);
                return View(model);
            }

            var oficina = new Oficina
            {
                Nome = model.Nome,
                Descricao = model.Descricao,
                GrupoOficinaId = grupo.Id,
                Plano = model.Plano,
                AdminProprietarioId = model.AdminId,
                CorPrimaria = string.IsNullOrWhiteSpace(model.CorPrimaria) ? grupo.CorPrimaria : model.CorPrimaria!,
                CorSecundaria = string.IsNullOrWhiteSpace(model.CorSecundaria) ? grupo.CorSecundaria : model.CorSecundaria!,
                FinanceiroPrazoSemJurosDias = grupo.Oficinas.FirstOrDefault()?.FinanceiroPrazoSemJurosDias ?? 90,
                FinanceiroJurosMensal = grupo.Oficinas.FirstOrDefault()?.FinanceiroJurosMensal ?? 0.02m
            };

            _db.Oficinas.Add(oficina);
            await _db.SaveChangesAsync();
            await CriarRecursosFinanceirosPadraoAsync(oficina);

            TempData["Msg"] = "Oficina criada com sucesso.";
            return RedirectToAction("Index", "AdminOficinas");
        }

        private async Task PopularDadosNovaOficinaAsync(NovaOficinaViewModel model)
        {
            var gruposQuery = _db.Grupos
                .Include(g => g.Oficinas)
                .AsQueryable();

            if (User.IsInRole("Diretor"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    gruposQuery = gruposQuery.Where(g => g.DiretorId == user.Id);
                }
            }

            var grupos = await gruposQuery
                .OrderBy(g => g.Nome)
                .Select(g => new
                {
                    g.Id,
                    g.Nome,
                    g.Plano,
                    Oficinas = g.Oficinas.Count
                })
                .ToListAsync();

            model.Grupos = grupos.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = $"{g.Nome} ({ObterPlanoLabel(g.Plano)})"
            }).ToList();

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            model.Admins = admins
                .OrderBy(u => u.NomeCompleto)
                .Select(a => new SelectListItem
                {
                    Value = a.Id,
                    Text = string.IsNullOrWhiteSpace(a.NomeCompleto) ? a.Email : a.NomeCompleto
                })
                .ToList();
        }

        private async Task<GrupoOficina?> ObterGrupoPermitidoAsync(int grupoId)
        {
            var query = _db.Grupos
                .Include(g => g.Oficinas)
                .Where(g => g.Id == grupoId);

            if (User.IsInRole("Diretor"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return null;
                query = query.Where(g => g.DiretorId == user.Id);
            }

            return await query.FirstOrDefaultAsync();
        }

        private async Task CriarRecursosFinanceirosPadraoAsync(Oficina oficina)
        {
            if (!await _db.ContasFinanceiras.AnyAsync(c => c.OficinaId == oficina.Id))
            {
                _db.ContasFinanceiras.AddRange(
                    new ContaFinanceira
                    {
                        OficinaId = oficina.Id,
                        Nome = "Caixa Principal",
                        Tipo = FinanceiroTipoConta.Caixa,
                        SaldoInicial = 0,
                        Ativo = true
                    },
                    new ContaFinanceira
                    {
                        OficinaId = oficina.Id,
                        Nome = "Banco Padrão",
                        Tipo = FinanceiroTipoConta.Banco,
                        Banco = "000",
                        Agencia = "0000",
                        NumeroConta = "000000-0",
                        SaldoInicial = 0,
                        Ativo = true
                    });
                await _db.SaveChangesAsync();
            }

            if (!await _db.CategoriasFinanceiras.AnyAsync(c => c.OficinaId == oficina.Id))
            {
                _db.CategoriasFinanceiras.AddRange(
                    new CategoriaFinanceira
                    {
                        OficinaId = oficina.Id,
                        Nome = "Serviços de Oficina",
                        Tipo = FinanceiroTipoLancamento.Receita,
                        Descricao = "Receitas geradas por ordens de serviço."
                    },
                    new CategoriaFinanceira
                    {
                        OficinaId = oficina.Id,
                        Nome = "Compra de Peças",
                        Tipo = FinanceiroTipoLancamento.Despesa,
                        Descricao = "Reposição de estoque e insumos."
                    },
                    new CategoriaFinanceira
                    {
                        OficinaId = oficina.Id,
                        Nome = "Comissões",
                        Tipo = FinanceiroTipoLancamento.Despesa,
                        Descricao = "Pagamentos de comissões para equipe."
                    });
                await _db.SaveChangesAsync();
            }
        }
    }
}
