using System;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using Services;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    [PlanoRecurso(PlanoConta.Pro, "Movimentações de Estoque")]
    public class MovimentacaoEstoqueController : Controller
    {
        private readonly OficinaDbContext _db;
        private readonly IEstoqueService _estoqueService;
        private readonly IOficinaContext _oficinaContext;

        public MovimentacaoEstoqueController(OficinaDbContext db, IEstoqueService estoqueService, IOficinaContext oficinaContext)
        {
            _db = db;
            _estoqueService = estoqueService;
            _oficinaContext = oficinaContext;
        }

        public async Task<IActionResult> Index(int? pecaId)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var query = _db.MovimentacoesEstoque
                .Include(m => m.PecaEstoque)
                .AsNoTracking()
                .Where(m => m.OficinaId == oficinaId)
                .OrderByDescending(m => m.DataMovimentacao);

            if (pecaId.HasValue)
            {
                var pertence = await _db.PecaEstoques.AnyAsync(p => p.Id == pecaId.Value && p.OficinaId == oficinaId);
                if (!pertence)
                {
                    return NotFound();
                }

                query = query.Where(m => m.PecaEstoqueId == pecaId.Value)
                    .OrderByDescending(m => m.DataMovimentacao);
                ViewBag.PecaSelecionada = pecaId.Value;
            }

            ViewBag.Pecas = await _db.PecaEstoques.AsNoTracking()
                .Where(p => p.OficinaId == oficinaId)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            var lista = await query.Take(200).ToListAsync();
            return View(lista);
        }

        public async Task<IActionResult> Entrada()
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            await PopularPecas(oficinaId);
            return View(new MovimentacaoEntradaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrada(MovimentacaoEntradaViewModel model)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            if (!await PecaPertenceAoGrupoAsync(model.PecaEstoqueId, oficinaId))
            {
                ModelState.AddModelError(nameof(model.PecaEstoqueId), "Peça inválida para esta oficina.");
            }

            if (!ModelState.IsValid)
            {
                await PopularPecas(oficinaId);
                return View(model);
            }

            try
            {
                await _estoqueService.RegistrarEntradaAsync(model.PecaEstoqueId, model.Quantidade, model.ValorUnitario, model.Observacao);
                TempData["Msg"] = "Entrada registrada com sucesso.";
                return RedirectToAction(nameof(Index), new { pecaId = model.PecaEstoqueId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopularPecas(oficinaId);
                return View(model);
            }
        }

        public async Task<IActionResult> Saida()
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            await PopularPecas(oficinaId);
            return View(new MovimentacaoSaidaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Saida(MovimentacaoSaidaViewModel model)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            if (!await PecaPertenceAoGrupoAsync(model.PecaEstoqueId, oficinaId))
            {
                ModelState.AddModelError(nameof(model.PecaEstoqueId), "Peça inválida para esta oficina.");
            }

            if (!ModelState.IsValid)
            {
                await PopularPecas(oficinaId);
                return View(model);
            }

            try
            {
                await _estoqueService.RegistrarSaidaAsync(model.PecaEstoqueId, model.Quantidade, model.Observacao);
                TempData["Msg"] = "Saída registrada com sucesso.";
                return RedirectToAction(nameof(Index), new { pecaId = model.PecaEstoqueId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopularPecas(oficinaId);
                return View(model);
            }
        }

        private async Task PopularPecas(int oficinaId)
        {
            ViewBag.Pecas = await _db.PecaEstoques.AsNoTracking()
                .Where(p => p.OficinaId == oficinaId)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        private async Task<bool> PecaPertenceAoGrupoAsync(int pecaId, int oficinaId)
        {
            return await _db.PecaEstoques.AnyAsync(p => p.Id == pecaId && p.OficinaId == oficinaId);
        }

        private async Task<int> ObterOficinaAtualIdAsync()
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null)
            {
                throw new InvalidOperationException("Nenhuma oficina selecionada.");
            }

            return oficina.Id;
        }
    }
}
