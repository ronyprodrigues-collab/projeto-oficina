using System;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace projetos.Controllers
{
    public class PainelController : Controller
    {
        private readonly OficinaDbContext _context;

        public PainelController(OficinaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            var emExecucao = await _context.OrdensServico
                .CountAsync(o => o.DataConclusao == null && o.AprovadaCliente);

            var concluidas = await _context.OrdensServico
                .CountAsync(o => o.DataConclusao != null);

            var pendentesAprovacao = await _context.OrdensServico
                .CountAsync(o => !o.AprovadaCliente);

            var servicosMes = await _context.ServicoItem
                .Where(si => si.OrdemServico.DataConclusao >= monthStart && si.OrdemServico.DataConclusao < nextMonthStart)
                .SumAsync(si => (decimal?)si.Valor) ?? 0m;

            var pecasMes = await _context.PecaItem
                .Where(pi => pi.OrdemServico.DataConclusao >= monthStart && pi.OrdemServico.DataConclusao < nextMonthStart)
                .SumAsync(pi => (decimal?)(pi.ValorUnitario * pi.Quantidade)) ?? 0m;

            var faturamentoMes = servicosMes + pecasMes;
            if (faturamentoMes == 0)
            {
                faturamentoMes = 12500.00m; // mock fallback
            }

            ViewData["EmExecucao"] = emExecucao;
            ViewData["Concluidas"] = concluidas;
            ViewData["PendentesAprovacao"] = pendentesAprovacao;
            ViewData["FaturamentoMes"] = faturamentoMes;

            var labels = Enumerable.Range(0, 6)
                .Select(i => monthStart.AddMonths(-i))
                .Reverse()
                .Select(d => d.ToString("MM/yyyy"))
                .ToArray();

            var rnd = new Random(2025);
            var values = labels.Select(_ => Math.Round((decimal)rnd.Next(8000, 22000), 2)).ToArray();

            ViewBag.ChartLabels = labels;
            ViewBag.ChartValues = values;

            return View();
        }
    }
}
