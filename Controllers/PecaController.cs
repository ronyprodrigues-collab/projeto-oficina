using System.Linq;
using System.Threading.Tasks;
using Data;
using Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    [PlanoRecurso(PlanoConta.Pro, "Módulo de Estoque")]
    public class PecaController : Controller
    {
        private readonly OficinaDbContext _db;

        public PecaController(OficinaDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var lista = await _db.PecaEstoques.AsNoTracking()
                .OrderBy(p => p.Nome)
                .ToListAsync();
            return View(lista);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PecaEstoque model)
        {
            model.UnidadeMedida = (model.UnidadeMedida ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(model.UnidadeMedida))
            {
                ModelState.AddModelError(nameof(model.UnidadeMedida), "Informe a unidade de medida.");
            }

            if (!ModelState.IsValid)
                return View(model);

            model.UnidadeMedida = model.UnidadeMedida.ToLowerInvariant();
            model.SaldoAtual = 0;
            model.OficinaId = 1; // TODO: substituir pelo contexto da oficina selecionada
            _db.PecaEstoques.Add(model);
            await _db.SaveChangesAsync();
            TempData["Msg"] = "Peça cadastrada.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.PecaEstoques.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PecaEstoque model)
        {
            if (id != model.Id) return BadRequest();

            model.UnidadeMedida = (model.UnidadeMedida ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(model.UnidadeMedida))
            {
                ModelState.AddModelError(nameof(model.UnidadeMedida), "Informe a unidade de medida.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var entity = await _db.PecaEstoques.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Nome = model.Nome;
            entity.Codigo = model.Codigo;
            entity.UnidadeMedida = model.UnidadeMedida.ToLowerInvariant();
            entity.EstoqueMinimo = model.EstoqueMinimo;
            entity.PrecoVenda = model.PrecoVenda;

            await _db.SaveChangesAsync();
            TempData["Msg"] = "Peça atualizada.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.PecaEstoques.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.PecaEstoques.FindAsync(id);
            if (item == null) return NotFound();

            var possuiMovimento = await _db.MovimentacoesEstoque.AnyAsync(m => m.PecaEstoqueId == id);
            if (possuiMovimento)
            {
                ModelState.AddModelError(string.Empty, "Não é possível excluir uma peça com movimentações registradas.");
                return View("Delete", item);
            }

            _db.PecaEstoques.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Msg"] = "Peça removida.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetPecaInfo(int id)
        {
            var peca = await _db.PecaEstoques.AsNoTracking()
                .Include(p => p.Movimentacoes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (peca == null) return NotFound();

            var ultimoValorEntrada = peca.Movimentacoes
                .Where(m => m.Tipo == "Entrada")
                .OrderByDescending(m => m.DataMovimentacao)
                .Select(m => (decimal?)m.ValorUnitario)
                .FirstOrDefault();

            var valorBase = peca.PrecoVenda ?? ultimoValorEntrada ?? 0m;

            return Json(new
            {
                nome = peca.Nome,
                valor = valorBase,
                unidade = peca.UnidadeMedida,
                saldo = peca.SaldoAtual
            });
        }
    }
}
