using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services
{
    public class EstoqueService : IEstoqueService
    {
        private readonly OficinaDbContext _db;

        public EstoqueService(OficinaDbContext db)
        {
            _db = db;
        }

        public async Task<PecaEstoque> RegistrarEntradaAsync(int pecaId, decimal quantidade, decimal valorUnitario, string? observacao, CancellationToken cancellationToken = default)
        {
            if (quantidade <= 0) throw new ArgumentOutOfRangeException(nameof(quantidade));
            if (valorUnitario < 0) throw new ArgumentOutOfRangeException(nameof(valorUnitario));

            var peca = await _db.PecaEstoques.FirstOrDefaultAsync(p => p.Id == pecaId, cancellationToken)
                       ?? throw new InvalidOperationException("Peça não encontrada.");

            var movimento = new MovimentacaoEstoque
            {
                PecaEstoqueId = pecaId,
                Tipo = "Entrada",
                Quantidade = quantidade,
                QuantidadeRestante = quantidade,
                ValorUnitario = valorUnitario,
                Observacao = observacao,
                DataMovimentacao = DateTime.UtcNow,
                OficinaId = peca.OficinaId
            };

            _db.MovimentacoesEstoque.Add(movimento);
            peca.SaldoAtual += quantidade;
            await _db.SaveChangesAsync(cancellationToken);
            return peca;
        }

        public async Task RegistrarSaidaAsync(int pecaId, decimal quantidade, string? observacao, int? ordemServicoId = null, CancellationToken cancellationToken = default)
        {
            if (quantidade <= 0) throw new ArgumentOutOfRangeException(nameof(quantidade));

            var peca = await _db.PecaEstoques.FirstOrDefaultAsync(p => p.Id == pecaId, cancellationToken)
                       ?? throw new InvalidOperationException("Peça não encontrada.");

            if (peca.SaldoAtual < quantidade)
            {
                throw new InvalidOperationException("Saldo insuficiente para a saída solicitada.");
            }

            var entradas = await _db.MovimentacoesEstoque
                .Where(m => m.PecaEstoqueId == pecaId && m.Tipo == "Entrada" && m.QuantidadeRestante > 0)
                .OrderBy(m => m.DataMovimentacao)
                .ThenBy(m => m.Id)
                .ToListAsync(cancellationToken);

            var restante = quantidade;
            foreach (var entrada in entradas)
            {
                if (restante <= 0) break;

                var consumido = Math.Min(entrada.QuantidadeRestante, restante);
                entrada.QuantidadeRestante -= consumido;

                _db.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                {
                    PecaEstoqueId = pecaId,
                    Tipo = "Saida",
                    Quantidade = consumido,
                    QuantidadeRestante = 0,
                    ValorUnitario = entrada.ValorUnitario,
                    Observacao = observacao,
                    OrdemServicoId = ordemServicoId,
                    MovimentacaoEntradaReferenciaId = entrada.Id,
                    DataMovimentacao = DateTime.UtcNow,
                    OficinaId = peca.OficinaId
                });

                restante -= consumido;
            }

            if (restante > 0)
            {
                throw new InvalidOperationException("Não foi possível completar a baixa utilizando o estoque FIFO.");
            }

            peca.SaldoAtual -= quantidade;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
