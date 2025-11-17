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

        public MovimentacaoEstoqueController(OficinaDbContext db, IEstoqueService estoqueService)
        {
            _db = db;
            _estoqueService = estoqueService;
        }

        public async Task<IActionResult> Index(int? pecaId)
        {
            var query = _db.MovimentacoesEstoque
                .Include(m => m.PecaEstoque)
                .AsNoTracking()
                .OrderByDescending(m => m.DataMovimentacao);

            if (pecaId.HasValue)
            {
                query = query.Where(m => m.PecaEstoqueId == pecaId.Value)
                    .OrderByDescending(m => m.DataMovimentacao);
                ViewBag.PecaSelecionada = pecaId.Value;
            }

            ViewBag.Pecas = await _db.PecaEstoques.AsNoTracking()
                .OrderBy(p => p.Nome)
                .ToListAsync();

            var lista = await query.Take(200).ToListAsync();
            return View(lista);
        }

        public async Task<IActionResult> Entrada()
        {
            await PopularPecas();
            return View(new MovimentacaoEntradaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrada(MovimentacaoEntradaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopularPecas();
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
                await PopularPecas();
                return View(model);
            }
        }

        public async Task<IActionResult> Saida()
        {
            await PopularPecas();
            return View(new MovimentacaoSaidaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Saida(MovimentacaoSaidaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopularPecas();
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
                await PopularPecas();
                return View(model);
            }
        }

        private async Task PopularPecas()
        {
            ViewBag.Pecas = await _db.PecaEstoques.AsNoTracking()
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }
    }
}
