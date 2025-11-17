using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public OficinasController(OficinaDbContext db, IOficinaContext oficinaContext)
        {
            _db = db;
            _oficinaContext = oficinaContext;
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
    }
}
