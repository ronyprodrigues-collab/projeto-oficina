using System;
using System.Collections.Generic;

namespace Models
{
    public class LancamentoFinanceiro : ISoftDeletable
    {
        public int Id { get; set; }
        public int OficinaId { get; set; }
        public Oficina Oficina { get; set; } = null!;

        public FinanceiroTipoLancamento Tipo { get; set; } = FinanceiroTipoLancamento.Despesa;
        public int CategoriaFinanceiraId { get; set; }
        public CategoriaFinanceira Categoria { get; set; } = null!;

        public int? ContaPadraoId { get; set; }
        public ContaFinanceira? ContaPadrao { get; set; }

        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public string? ParceiroNome { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string? NumeroDocumento { get; set; }
        public DateTime DataCompetencia { get; set; } = DateTime.UtcNow;
        public decimal ValorTotal { get; set; }
        public string? Origem { get; set; }
        public string? Observacao { get; set; }

        public ICollection<LancamentoParcela> Parcelas { get; set; } = new List<LancamentoParcela>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class LancamentoParcela : ISoftDeletable
    {
        public int Id { get; set; }
        public int LancamentoFinanceiroId { get; set; }
        public LancamentoFinanceiro Lancamento { get; set; } = null!;

        public int Numero { get; set; }
        public DateTime DataVencimento { get; set; }
        public decimal Valor { get; set; }
        public FinanceiroSituacaoParcela Situacao { get; set; } = FinanceiroSituacaoParcela.Pendente;
        public DateTime? DataPagamento { get; set; }
        public int? ContaPagamentoId { get; set; }
        public ContaFinanceira? ContaPagamento { get; set; }
        public string? Observacao { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
