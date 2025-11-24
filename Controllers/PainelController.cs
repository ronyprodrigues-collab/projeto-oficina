using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Services;

namespace projetos.Controllers
{
    [Authorize]
    public class PainelController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly IOficinaContext _oficinaContext;

        public PainelController(OficinaDbContext context, IOficinaContext oficinaContext)
        {
            _context = context;
            _oficinaContext = oficinaContext;
        }

        public async Task<IActionResult> Index(int? mes, int? ano, string? mecanicoId, int? clienteId)
        {
            var now = DateTime.Now;

            var oficinaAtual = await _oficinaContext.GetOficinaAtualAsync();
            if (oficinaAtual == null)
            {
                TempData["Error"] = "Selecione uma oficina para visualizar o painel.";
                return RedirectToAction("Selecionar", "Oficinas", new { returnUrl = Url.Action(nameof(Index)) });
            }

            var grupoId = oficinaAtual.GrupoOficinaId;
            ViewData["GrupoAtualNome"] = oficinaAtual.Grupo?.Nome ?? oficinaAtual.Nome;

            var mecanicosDisponiveis = await GetMecanicosAsync(grupoId);
            if (!string.IsNullOrEmpty(mecanicoId) && !mecanicosDisponiveis.Any(m => m.Id == mecanicoId))
            {
                mecanicoId = null;
            }

            if (clienteId.HasValue)
            {
                var clientePertence = await _context.Clientes
                    .AnyAsync(c => c.Id == clienteId.Value && c.Oficinas.Any(oc => oc.Oficina.GrupoOficinaId == grupoId));
                if (!clientePertence)
                {
                    clienteId = null;
                }
            }

            var monthStart = new DateTime(ano ?? now.Year, mes ?? now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            var ordensGrupo = _context.OrdensServico.Where(o => o.Oficina.GrupoOficinaId == grupoId);
            var servicosGrupo = _context.ServicoItem.Where(si => si.OrdemServico.Oficina.GrupoOficinaId == grupoId);
            var pecasGrupo = _context.PecaItem.Where(pi => pi.OrdemServico.Oficina.GrupoOficinaId == grupoId);

            // ------------------------------------------------------
            // 1) CARDS
            // ------------------------------------------------------
            var emExecucao = await ordensGrupo
                .Where(o => o.DataConclusao == null && o.AprovadaCliente)
                .CountAsync();

            var concluidas = await ordensGrupo
                .Where(o => o.DataConclusao != null)
                .CountAsync();

            var pendentesAprovacao = await ordensGrupo
                .Where(o => !o.AprovadaCliente && o.DataConclusao == null)
                .CountAsync();

            var servicosMes = await servicosGrupo
                .Where(si => si.OrdemServico.DataConclusao != null)
                .Where(si => si.OrdemServico.DataConclusao >= monthStart && si.OrdemServico.DataConclusao < nextMonthStart)
                .Where(si =>
                    (string.IsNullOrWhiteSpace(mecanicoId) || si.OrdemServico.MecanicoId == mecanicoId) &&
                    (!clienteId.HasValue || si.OrdemServico.ClienteId == clienteId.Value))
                .SumAsync(si => (decimal?)si.Valor) ?? 0m;

            var pecasMes = await pecasGrupo
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

                var fServ = await servicosGrupo
                    .Where(si => si.OrdemServico.DataConclusao != null &&
                                 si.OrdemServico.DataConclusao >= d &&
                                 si.OrdemServico.DataConclusao < d2)
                    .SumAsync(si => (decimal?)si.Valor) ?? 0m;

                var fPec = await pecasGrupo
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
            var statusDistrib = await ordensGrupo
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Total = g.Count() })
                .ToListAsync();

            ViewBag.StatusLabels = statusDistrib.Select(s => s.Status).ToArray();
            ViewBag.StatusValues = statusDistrib.Select(s => s.Total).ToArray();

            // ------------------------------------------------------
            // 4) TOP 5 SERVIÇOS MAIS EXECUTADOS
            // ------------------------------------------------------
            var topServ = await servicosGrupo
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

                servMensal.Add(await servicosGrupo
                    .Where(si => si.OrdemServico.DataConclusao != null &&
                                 si.OrdemServico.DataConclusao >= d &&
                                 si.OrdemServico.DataConclusao < d2)
                    .SumAsync(si => (decimal?)si.Valor) ?? 0m);

                pecMensal.Add(await pecasGrupo
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
            var osPorMec = await ordensGrupo
                .Where(o => o.MecanicoId != null)
                .GroupBy(o => o.Mecanico != null ? o.Mecanico.NomeCompleto : "Sem Nome")
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

                aberturas.Add(await ordensGrupo
                    .CountAsync(o => o.DataAbertura >= d && o.DataAbertura < d2));

                conclusoes.Add(await ordensGrupo
                    .CountAsync(o => o.DataConclusao != null &&
                                     o.DataConclusao >= d &&
                                     o.DataConclusao < d2));
            }

            ViewBag.Aberturas = aberturas;
            ViewBag.Conclusoes = conclusoes;

            // ------------------------------------------------------
            // 8) FATURAMENTO POR CLIENTE (MÊS)
            // ------------------------------------------------------
            var faturamentoPorClienteBruto = await ordensGrupo
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

            ViewBag.Mecanicos = mecanicosDisponiveis;

            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Oficinas.Any(oc => oc.Oficina.GrupoOficinaId == grupoId))
                .OrderBy(c => c.Nome ?? "(Sem nome)")
                .ToListAsync();

            return View();
        }

        private async Task<List<ApplicationUser>> GetMecanicosAsync(int grupoId)
        {
            return await _context.OficinasUsuarios
                .Where(ou => ou.Perfil == "Mecanico" && ou.Ativo && ou.Oficina.GrupoOficinaId == grupoId)
                .Select(ou => ou.Usuario)
                .Distinct()
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();
        }
    }
}
