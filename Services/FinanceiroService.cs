using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;

namespace Services
{
    public class FinanceiroService : IFinanceiroService
    {
        private readonly OficinaDbContext _db;

        public FinanceiroService(OficinaDbContext db)
        {
            _db = db;
        }

        public async Task<FinanceiroDashboardViewModel> ObterDashboardAsync(int oficinaId, CancellationToken cancellationToken = default)
        {
            var contas = await _db.ContasFinanceiras
                .Where(c => c.OficinaId == oficinaId)
                .OrderBy(c => c.Nome)
                .Select(c => new ContaFinanceiraResumoViewModel
                {
                    ContaId = c.Id,
                    Nome = c.Nome,
                    Tipo = c.Tipo,
                    SaldoInicial = c.SaldoInicial
                })
                .ToListAsync(cancellationToken);

            var movimentosPorConta = await _db.LancamentoParcelas
                .AsNoTracking()
                .Where(p => p.Lancamento.OficinaId == oficinaId
                    && p.Situacao == FinanceiroSituacaoParcela.Pago
                    && p.ContaPagamentoId != null)
                .GroupBy(p => new { p.ContaPagamentoId, p.Lancamento.Tipo })
                .Select(g => new
                {
                    ContaId = g.Key.ContaPagamentoId!.Value,
                    Tipo = g.Key.Tipo,
                    Valor = g.Sum(x => x.Valor)
                })
                .ToListAsync(cancellationToken);

            foreach (var conta in contas)
            {
                var entradas = movimentosPorConta
                    .Where(m => m.ContaId == conta.ContaId && m.Tipo == FinanceiroTipoLancamento.Receita)
                    .Sum(m => m.Valor);
                var saidas = movimentosPorConta
                    .Where(m => m.ContaId == conta.ContaId && m.Tipo == FinanceiroTipoLancamento.Despesa)
                    .Sum(m => m.Valor);
                conta.Entradas = entradas;
                conta.Saidas = saidas;
            }

            var vm = new FinanceiroDashboardViewModel
            {
                Contas = contas
            };

            vm.PendentesReceber = (await ObterPendenciasAsync(oficinaId, FinanceiroTipoLancamento.Receita, 5, cancellationToken)).ToList();
            vm.PendentesPagar = (await ObterPendenciasAsync(oficinaId, FinanceiroTipoLancamento.Despesa, 5, cancellationToken)).ToList();

            vm.OsRecentes = await _db.OrdensServico
                .AsNoTracking()
                .Where(o => o.OficinaId == oficinaId && o.DataConclusao != null)
                .OrderByDescending(o => o.DataConclusao)
                .Take(6)
                .Select(o => new OrdemServicoFinanceiroViewModel
                {
                    OrdemServicoId = o.Id,
                    Cliente = o.Cliente != null ? o.Cliente.Nome : "Cliente não definido",
                    DataConclusao = o.DataConclusao,
                    ValorTotal = (o.Servicos.Sum(s => (decimal?)s.Valor) ?? 0m) + (o.Pecas.Sum(p => (decimal?)(p.ValorUnitario * p.Quantidade)) ?? 0m),
                    FormaPagamento = o.FormaPagamento,
                    ContaDestino = o.ContaRecebimento != null ? o.ContaRecebimento.Nome : null,
                    LancamentoGerado = o.LancamentoFinanceiroReceitaId != null
                })
                .ToListAsync(cancellationToken);

            vm.TotalReceber = vm.PendentesReceber.Sum(p => p.Valor);
            vm.TotalPagar = vm.PendentesPagar.Sum(p => p.Valor);

            return vm;
        }

        public async Task<IReadOnlyList<LancamentoResumoViewModel>> ObterPendenciasAsync(int oficinaId, FinanceiroTipoLancamento tipo, int limite = 5, CancellationToken cancellationToken = default)
        {
            var hoje = DateTime.Today;
            var query = _db.LancamentoParcelas
                .AsNoTracking()
                .Where(p => p.Lancamento.OficinaId == oficinaId
                            && p.Lancamento.Tipo == tipo
                            && p.Situacao == FinanceiroSituacaoParcela.Pendente)
                .OrderBy(p => p.DataVencimento)
                .Take(limite);

            var lista = await query
                .Select(p => new LancamentoResumoViewModel
                {
                    ParcelaId = p.Id,
                    LancamentoId = p.LancamentoFinanceiroId,
                    Descricao = p.Lancamento.Descricao,
                    Categoria = p.Lancamento.Categoria.Nome,
                    DataVencimento = p.DataVencimento,
                    Valor = p.Valor,
                    EmAtraso = p.DataVencimento.Date < hoje,
                    Tipo = tipo
                })
                .ToListAsync(cancellationToken);

            return lista;
        }
    }
}
