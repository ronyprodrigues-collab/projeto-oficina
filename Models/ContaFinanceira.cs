using System;
using System.Collections.Generic;

namespace Models
{
    public class ContaFinanceira : ISoftDeletable
    {
        public int Id { get; set; }
        public int OficinaId { get; set; }
        public Oficina Oficina { get; set; } = null!;
        public string Nome { get; set; } = string.Empty;
        public FinanceiroTipoConta Tipo { get; set; } = FinanceiroTipoConta.Caixa;
        public decimal SaldoInicial { get; set; }
        public string? Banco { get; set; }
        public string? Agencia { get; set; }
        public string? NumeroConta { get; set; }
        public bool Ativo { get; set; } = true;

        public ICollection<LancamentoFinanceiro> LancamentosPadrao { get; set; } = new List<LancamentoFinanceiro>();
        public ICollection<LancamentoParcela> ParcelasPagas { get; set; } = new List<LancamentoParcela>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
