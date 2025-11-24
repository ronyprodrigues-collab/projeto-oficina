using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Services;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ContasFinanceirasController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly IOficinaContext _oficinaContext;

        public ContasFinanceirasController(OficinaDbContext context, IOficinaContext oficinaContext)
        {
            _context = context;
            _oficinaContext = oficinaContext;
        }

        public async Task<IActionResult> Index()
        {
            var (oficina, redirect) = await ObterOficinaFinanceiroAsync();
            if (redirect != null) return redirect;

            ViewBag.OficinaNome = oficina!.Nome;
            var contas = await _context.ContasFinanceiras
                .Where(c => c.OficinaId == oficina.Id)
                .OrderBy(c => c.Nome)
                .ToListAsync();
            return View(contas);
        }

        public async Task<IActionResult> Create()
        {
            var (_, redirect) = await ObterOficinaFinanceiroAsync();
            if (redirect != null) return redirect;
            return View(new ContaFinanceira());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContaFinanceira model)
        {
            var (oficina, redirect) = await ObterOficinaFinanceiroAsync();
            if (redirect != null) return redirect;

            if (!ModelState.IsValid) return View(model);
            model.OficinaId = oficina!.Id;
            _context.ContasFinanceiras.Add(model);
            await _context.SaveChangesAsync();
            TempData["Msg"] = "Conta cadastrada.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var (oficina, redirect) = await ObterOficinaFinanceiroAsync();
            if (redirect != null) return redirect;

            var conta = await _context.ContasFinanceiras.FirstOrDefaultAsync(c => c.Id == id && c.OficinaId == oficina!.Id);
            if (conta == null) return NotFound();
            return View(conta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContaFinanceira model)
        {
            var (oficina, redirect) = await ObterOficinaFinanceiroAsync();
            if (redirect != null) return redirect;

            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var conta = await _context.ContasFinanceiras.FirstOrDefaultAsync(c => c.Id == id && c.OficinaId == oficina!.Id);
            if (conta == null) return NotFound();

            conta.Nome = model.Nome;
            conta.Tipo = model.Tipo;
            conta.SaldoInicial = model.SaldoInicial;
            conta.Banco = model.Banco;
            conta.Agencia = model.Agencia;
            conta.NumeroConta = model.NumeroConta;
            conta.Ativo = model.Ativo;
            await _context.SaveChangesAsync();
            TempData["Msg"] = "Conta atualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var (oficina, redirect) = await ObterOficinaFinanceiroAsync();
            if (redirect != null) return redirect;

            var conta = await _context.ContasFinanceiras.FirstOrDefaultAsync(c => c.Id == id && c.OficinaId == oficina!.Id);
            if (conta == null) return NotFound();
            conta.Ativo = !conta.Ativo;
            await _context.SaveChangesAsync();
            TempData["Msg"] = conta.Ativo ? "Conta ativada." : "Conta desativada.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<(Oficina? oficina, IActionResult? redirect)> ObterOficinaFinanceiroAsync()
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null)
            {
                TempData["Error"] = "Selecione uma oficina para acessar o módulo financeiro.";
                return (null, RedirectToAction("Selecionar", "Oficinas"));
            }

            if (oficina.Plano < PlanoConta.Plus)
            {
                TempData["Error"] = "O módulo financeiro está disponível apenas no Plano Plus.";
                return (null, RedirectToAction("Index", "Painel"));
            }

            return (oficina, null);
        }
    }
}
