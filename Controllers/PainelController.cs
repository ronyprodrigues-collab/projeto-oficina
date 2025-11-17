using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace projetos.Controllers
{
    [Authorize]
    public class PainelController : Controller
    {
        private readonly OficinaDbContext _context;

        public PainelController(OficinaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? mes, int? ano, string? mecanicoId, int? clienteId)
        {
            var now = DateTime.Now;

            var monthStart = new DateTime(ano ?? now.Year, mes ?? now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            // ------------------------------------------------------
            // 1) CARDS
            // ------------------------------------------------------
            var emExecucao = await _context.OrdensServico
                .CountAsync(o => o.DataConclusao == null && o.AprovadaCliente);

            var concluidas = await _context.OrdensServico
                .CountAsync(o => o.DataConclusao != null);

            var pendentesAprovacao = await _context.OrdensServico
                .CountAsync(o => !o.AprovadaCliente && o.DataConclusao == null);

            var servicosMes = await _context.ServicoItem
                .Where(si => si.OrdemServico.DataConclusao != null)
                .Where(si => si.OrdemServico.DataConclusao >= monthStart && si.OrdemServico.DataConclusao < nextMonthStart)
                .Where(si =>
                    (string.IsNullOrWhiteSpace(mecanicoId) || si.OrdemServico.MecanicoId == mecanicoId) &&
                    (!clienteId.HasValue || si.OrdemServico.ClienteId == clienteId.Value))
                .SumAsync(si => (decimal?)si.Valor) ?? 0m;

            var pecasMes = await _context.PecaItem
                .Where(pi => pi.OrdemServico.DataConclusao != null)
                .Where(pi => pi.OrdemServico.DataConclusao >= monthStart && pi.OrdemServico.DataConclusao < nextMonthStart)
                .Where(pi =>
                    (string.IsNullOrWhiteSpace(mecanicoId) || pi.OrdemServico.MecanicoId == mecanicoId) &&
                    (!clienteId.HasValue || pi.OrdemServico.ClienteId == clienteId.Value))
                .SumAsync(pi => (decimal?)(pi.ValorUnitario * pi.Quantidade)) ?? 0m;

            ViewData["FaturamentoMes"] = servicosMes + pecasMes;
            ViewData["EmExecucao"] = emExecucao;
            ViewData["Concluidas"] = concluidas;
            ViewData["PendentesAprovacao"] = pendentesAprovacao;

            // ------------------------------------------------------
            // 2) FATURAMENTO - últimos 6 meses
            // ------------------------------------------------------
            var last6 = Enumerable.Range(0, 6)
                .Select(i => monthStart.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var labels = last6.Select(d => d.ToString("MM/yyyy")).ToArray();
            var faturamento6 = new List<decimal>();

            foreach (var d in last6)
            {
                var d2 = d.AddMonths(1);

                var fServ = await _context.ServicoItem
                    .Where(si => si.OrdemServico.DataConclusao != null &&
                                 si.OrdemServico.DataConclusao >= d &&
                                 si.OrdemServico.DataConclusao < d2)
                    .SumAsync(si => (decimal?)si.Valor) ?? 0m;

                var fPec = await _context.PecaItem
                    .Where(pi => pi.OrdemServico.DataConclusao != null &&
                                 pi.OrdemServico.DataConclusao >= d &&
                                 pi.OrdemServico.DataConclusao < d2)
                    .SumAsync(pi => (decimal?)(pi.ValorUnitario * pi.Quantidade)) ?? 0m;

                faturamento6.Add(fServ + fPec);
            }

            ViewBag.ChartLabels = labels;
            ViewBag.ChartValues = faturamento6;

            // ------------------------------------------------------
            // 3) DISTRIBUIÇÃO DE STATUS (pizza)
            // ------------------------------------------------------
            var statusDistrib = await _context.OrdensServico
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Total = g.Count() })
                .ToListAsync();

            ViewBag.StatusLabels = statusDistrib.Select(s => s.Status).ToArray();
            ViewBag.StatusValues = statusDistrib.Select(s => s.Total).ToArray();

            // ------------------------------------------------------
            // 4) TOP 5 SERVIÇOS MAIS EXECUTADOS
            // ------------------------------------------------------
            var topServ = await _context.ServicoItem
                .GroupBy(s => s.Descricao)
                .Select(g => new { Servico = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .Take(5)
                .ToListAsync();

            ViewBag.TopServicosLabels = topServ.Select(s => s.Servico).ToArray();
            ViewBag.TopServicosValues = topServ.Select(s => s.Total).ToArray();

            // ------------------------------------------------------
            // 5) SERVIÇOS X PEÇAS (comparativo mensal)
            // ------------------------------------------------------
            var servMensal = new List<decimal>();
            var pecMensal = new List<decimal>();

            foreach (var d in last6)
            {
                var d2 = d.AddMonths(1);

                servMensal.Add(await _context.ServicoItem
                    .Where(si => si.OrdemServico.DataConclusao != null &&
                                 si.OrdemServico.DataConclusao >= d &&
                                 si.OrdemServico.DataConclusao < d2)
                    .SumAsync(si => (decimal?)si.Valor) ?? 0m);

                pecMensal.Add(await _context.PecaItem
                    .Where(pi => pi.OrdemServico.DataConclusao != null &&
                                 pi.OrdemServico.DataConclusao >= d &&
                                 pi.OrdemServico.DataConclusao < d2)
                    .SumAsync(pi => (decimal?)(pi.ValorUnitario * pi.Quantidade)) ?? 0m);
            }

            ViewBag.ServicosMensal = servMensal;
            ViewBag.PecasMensal = pecMensal;

            // ------------------------------------------------------
            // 6) OS POR MECÂNICO
            // ------------------------------------------------------
            var osPorMec = await _context.OrdensServico
                .Where(o => o.MecanicoId != null)
                .GroupBy(o => o.Mecanico.NomeCompleto)
                .Select(g => new { Nome = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToListAsync();

            ViewBag.MecLabels = osPorMec.Select(m => m.Nome ?? "Sem Nome").ToArray();
            ViewBag.MecValues = osPorMec.Select(m => m.Total).ToArray();

            // ------------------------------------------------------
            // 7) ABERTURA X CONCLUSÃO DE OS
            // ------------------------------------------------------
            var aberturas = new List<int>();
            var conclusoes = new List<int>();

            foreach (var d in last6)
            {
                var d2 = d.AddMonths(1);

                aberturas.Add(await _context.OrdensServico
                    .CountAsync(o => o.DataAbertura >= d && o.DataAbertura < d2));

                conclusoes.Add(await _context.OrdensServico
                    .CountAsync(o => o.DataConclusao != null &&
                                     o.DataConclusao >= d &&
                                     o.DataConclusao < d2));
            }

            ViewBag.Aberturas = aberturas;
            ViewBag.Conclusoes = conclusoes;

            // ------------------------------------------------------
            // 8) FATURAMENTO POR CLIENTE (MÊS)
            // ------------------------------------------------------
            var faturamentoPorClienteBruto = await _context.OrdensServico
                .Where(o => o.DataConclusao != null &&
                            o.DataConclusao >= monthStart &&
                            o.DataConclusao < nextMonthStart)
                .Where(o =>
                    (string.IsNullOrWhiteSpace(mecanicoId) || o.MecanicoId == mecanicoId) &&
                    (!clienteId.HasValue || o.ClienteId == clienteId.Value))
                .Select(o => new
                {
                    ClienteNome = o.Cliente.Nome ?? $"Cliente #{o.ClienteId}",
                    TotalServicos = o.Servicos.Sum(s => (decimal?)s.Valor) ?? 0m,
                    TotalPecas = o.Pecas.Sum(p => (decimal?)(p.ValorUnitario * p.Quantidade)) ?? 0m
                })
                .ToListAsync();

            var faturamentoPorCliente = faturamentoPorClienteBruto
                .GroupBy(x => x.ClienteNome)
                .Select(g => new
                {
                    Cliente = g.Key,
                    Total = g.Sum(x => x.TotalServicos + x.TotalPecas)
                })
                .OrderByDescending(g => g.Total)
                .Take(5)
                .ToList();

            ViewBag.FatClientesLabels = faturamentoPorCliente.Select(f => f.Cliente).ToArray();
            ViewBag.FatClientesValues = faturamentoPorCliente.Select(f => f.Total).ToArray();

            var topCliente = faturamentoPorCliente.FirstOrDefault();
            ViewData["TopClienteNome"] = topCliente?.Cliente ?? "Sem movimentação";
            ViewData["TopClienteValor"] = topCliente?.Total ?? 0m;

            // ------------------------------------------------------
            // FILTROS
            // ------------------------------------------------------
            ViewBag.SelectedMes = monthStart.Month;
            ViewBag.SelectedAno = monthStart.Year;
            ViewBag.SelectedMecanico = mecanicoId;
            ViewBag.SelectedCliente = clienteId;

            ViewBag.Meses = Enumerable.Range(1, 12).Select(i => new
            {
                Value = i,
                Text = new DateTime(2000, i, 1).ToString("MMMM")
            }).ToList();

            var anoAtual = now.Year;
            ViewBag.Anos = Enumerable.Range(anoAtual - 5, 6)
                .Select(a => new { Value = a, Text = a.ToString() })
                .ToList();

            ViewBag.Mecanicos = await GetMecanicosAsync();

            ViewBag.Clientes = await _context.Clientes
                .OrderBy(c => c.Nome ?? "(Sem nome)")
                .ToListAsync();

            return View();
        }

        private async Task<List<ApplicationUser>> GetMecanicosAsync()
        {
            var roleId = await _context.Roles
                .Where(r => r.Name == "Mecanico")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(roleId))
                return new List<ApplicationUser>();

            return await (from u in _context.Users
                          join ur in _context.UserRoles on u.Id equals ur.UserId
                          where ur.RoleId == roleId
                          orderby u.NomeCompleto
                          select u).ToListAsync();
        }
    }
}
