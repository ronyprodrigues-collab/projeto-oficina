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
        private readonly Services.IOficinaContext _oficinaContext;

        public EstoqueController(OficinaDbContext db, Services.IOficinaContext oficinaContext)
        {
            _db = db;
            _oficinaContext = oficinaContext;
        }

        public async Task<IActionResult> Index()
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var lista = await _db.PecaEstoques.AsNoTracking()
                .Where(p => p.OficinaId == oficinaId)
                .OrderBy(p => p.Nome)
                .ToListAsync();
            return View(lista);
        }

        public async Task<IActionResult> Details(int id)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var item = await _db.PecaEstoques
                .Include(p => p.Movimentacoes)
                .ThenInclude(m => m.MovimentacaoEntradaReferencia)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.OficinaId == oficinaId);

            if (item == null) return NotFound();

            item.Movimentacoes = item.Movimentacoes
                .OrderByDescending(m => m.DataMovimentacao)
                .ToList();

            return View(item);
        }

        private async Task<int> ObterOficinaAtualIdAsync()
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null)
            {
                throw new InvalidOperationException("Nenhuma oficina selecionada no contexto atual.");
            }

            return oficina.Id;
        }
    }
}
