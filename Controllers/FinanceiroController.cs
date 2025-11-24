using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Services;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FinanceiroController : Controller
    {
        private readonly IFinanceiroService _financeiroService;
        private readonly IOficinaContext _oficinaContext;

        public FinanceiroController(IFinanceiroService financeiroService, IOficinaContext oficinaContext)
        {
            _financeiroService = financeiroService;
            _oficinaContext = oficinaContext;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync(cancellationToken);
            if (oficina == null)
            {
                TempData["Error"] = "Selecione uma oficina para acessar o módulo financeiro.";
                return RedirectToAction("Selecionar", "Oficinas");
            }

            if (oficina.Plano < PlanoConta.Plus)
            {
                TempData["Error"] = "O painel financeiro está disponível apenas para oficinas no Plano Plus.";
                return RedirectToAction("Index", "Painel");
            }

            FinanceiroDashboardViewModel vm = await _financeiroService.ObterDashboardAsync(oficina.Id, cancellationToken);
            ViewBag.OficinaNome = oficina.Nome;
            return View(vm);
        }
    }
}
