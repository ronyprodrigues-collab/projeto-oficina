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
    public class EstoqueController : Controller
    {
        private readonly OficinaDbContext _db;

        public EstoqueController(OficinaDbContext db)
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

        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.PecaEstoques
                .Include(p => p.Movimentacoes)
                .ThenInclude(m => m.MovimentacaoEntradaReferencia)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (item == null) return NotFound();

            item.Movimentacoes = item.Movimentacoes
                .OrderByDescending(m => m.DataMovimentacao)
                .ToList();

            return View(item);
        }
    }
}
