using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using Services;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LancamentosFinanceirosController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly IOficinaContext _oficinaContext;

        public LancamentosFinanceirosController(OficinaDbContext context, IOficinaContext oficinaContext)
        {
            _context = context;
            _oficinaContext = oficinaContext;
        }

        public async Task<IActionResult> Index(FinanceiroTipoLancamento? tipo, FinanceiroSituacaoParcela? situacao, bool apenasAtrasados = false)
        {
            var (oficina, redirect) = await ObterOficinaFinanceiroAsync();
            if (redirect != null) return redirect;

            var query = _context.LancamentoParcelas
                .AsNoTracking()
                .Include(p => p.Lancamento)
                .ThenInclude(l => l.Categoria)
                .Where(p => p.Lancamento.OficinaId == oficina!.Id);

            if (tipo.HasValue)
            {
                query = query.Where(p => p.Lancamento.Tipo == tipo.Value);
            }
            if (situacao.HasValue)
            {
                query = query.Where(p => p.Situacao == situacao.Value);
            }

            if (apenasAtrasados)
            {
                var hoje = DateTime.Today;
                query = query.Where(p => p.Situacao == FinanceiroSituacaoParcela.Pendente && p.DataVencimento < hoje);
            }

            var itens = await query
                .OrderBy(p => p.DataVencimento)
                .Take(300)
                .Select(p => new LancamentoParcelaListItemViewModel
                {
                    ParcelaId = p.Id,
                    LancamentoId = p.LancamentoFinanceiroId,
                    Tipo = p.Lancamento.Tipo,
                    Descricao = p.Lancamento.Descricao,
                    Categoria = p.Lancamento.Categoria.Nome,
                    Valor = p.Valor,
                    DataVencimento = p.DataVencimento,
                    Situacao = p.Situacao,
                    DataPagamento = p.DataPagamento,
                    NumeroDocumento = p.Lancamento.NumeroDocumento,
                    ParceiroNome = p.Lancamento.ParceiroNome
                })
                .ToListAsync();

            var contasPagamento = await _context.ContasFinanceiras
                .Where(c => c.OficinaId == oficina!.Id)
                .OrderBy(c => c.Nome)
                .Select(c => new { c.Id, c.Nome })
                .ToListAsync();

            ViewBag.Tipo = tipo;
            ViewBag.Situacao = situacao;
            ViewBag.Atrasados = apenasAtrasados;
            ViewBag.OficinaNome = oficina!.Nome;
            ViewBag.ContasPagamento = contasPagamento;
            return View(itens);
        }

        [HttpGet]
        public async Task<IActionResult> Create(FinanceiroTipoLancamento? tipo)
        {
            var (oficina, redirect) = await ObterOficinaFinanceiroAsync();
            if (redirect != null) return redirect;

            var model = new LancamentoFinanceiroInputModel
            {
                Tipo = tipo ?? FinanceiroTipoLancamento.Despesa
            };
            await PopularSelectListsAsync(model, oficina!.Id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LancamentoFinanceiroInputModel model)
        {
            var (oficina, redirect) = await ObterOficinaFinanceiroAsync();
            if (redirect != null) return redirect;

            if (model.ValorParcela <= 0)
            {
                ModelState.AddModelError(nameof(model.ValorParcela), "Informe um valor válido.");
            }
            if (model.QuantidadeParcelas <= 0)
            {
                ModelState.AddModelError(nameof(model.QuantidadeParcelas), "Quantidade de parcelas deve ser maior que zero.");
            }
            if (!ModelState.IsValid)
            {
                await PopularSelectListsAsync(model, oficina!.Id);
                return View(model);
            }

            var lancamento = new LancamentoFinanceiro
            {
                OficinaId = oficina!.Id,
                Tipo = model.Tipo,
                CategoriaFinanceiraId = model.CategoriaFinanceiraId,
                ContaPadraoId = model.ContaPadraoId,
                ClienteId = model.Tipo == FinanceiroTipoLancamento.Receita ? model.ClienteId : null,
                ParceiroNome = model.ParceiroNome,
                Descricao = model.Descricao,
                NumeroDocumento = model.NumeroDocumento,
                DataCompetencia = model.DataCompetencia == default ? DateTime.Today : model.DataCompetencia,
                ValorTotal = model.ValorParcela * model.QuantidadeParcelas,
                Observacao = model.Observacao,
                FormaPagamento = model.FormaPagamento,
                QuantidadeParcelas = model.QuantidadeParcelas
            };

            var dataVencimento = model.DataPrimeiroVencimento == default ? DateTime.Today : model.DataPrimeiroVencimento;
            var intervalo = model.IntervaloDias <= 0 ? 30 : model.IntervaloDias;

            for (int i = 0; i < model.QuantidadeParcelas; i++)
            {
                lancamento.Parcelas.Add(new LancamentoParcela
                {
                    Numero = i + 1,
                    DataVencimento = dataVencimento.AddDays(intervalo * i),
                    Valor = model.ValorParcela,
                    Situacao = FinanceiroSituacaoParcela.Pendente
                });
            }

            _context.LancamentosFinanceiros.Add(lancamento);
            await _context.SaveChangesAsync();
            TempData["Msg"] = "Lançamento criado com sucesso.";
            return RedirectToAction(nameof(Index), new { tipo = model.Tipo });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BaixarParcela(int parcelaId, int contaPagamentoId, DateTime dataPagamento)
        {
            var (oficina, redirect) = await ObterOficinaFinanceiroAsync();
            if (redirect != null) return redirect;

            var parcela = await _context.LancamentoParcelas
                .Include(p => p.Lancamento)
                .FirstOrDefaultAsync(p => p.Id == parcelaId && p.Lancamento.OficinaId == oficina!.Id);
            if (parcela == null)
            {
                TempData["Error"] = "Parcela não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            parcela.Situacao = FinanceiroSituacaoParcela.Pago;
            parcela.DataPagamento = dataPagamento == default ? DateTime.Today : dataPagamento;
            parcela.ContaPagamentoId = contaPagamentoId;
            await _context.SaveChangesAsync();
            TempData["Msg"] = "Parcela baixada com sucesso.";
            return RedirectToAction(nameof(Index), new { tipo = parcela.Lancamento.Tipo });
        }

        private async Task PopularSelectListsAsync(LancamentoFinanceiroInputModel model, int oficinaId)
        {
            model.Categorias = await _context.CategoriasFinanceiras
                .Where(c => c.OficinaId == oficinaId && c.Tipo == model.Tipo)
                .OrderBy(c => c.Nome)
                .ToDictionaryAsync(c => c.Id, c => c.Nome);

            model.Contas = await _context.ContasFinanceiras
                .Where(c => c.OficinaId == oficinaId)
                .OrderBy(c => c.Nome)
                .ToDictionaryAsync(c => c.Id, c => c.Nome);

            if (model.Tipo == FinanceiroTipoLancamento.Receita)
            {
                model.Clientes = await _context.OficinasClientes
                    .Where(oc => oc.OficinaId == oficinaId)
                    .Select(oc => oc.Cliente)
                    .OrderBy(c => c.Nome)
                    .ToDictionaryAsync(c => c.Id, c => c.Nome);
            }
            else
            {
                model.Clientes = new Dictionary<int, string>();
            }
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
