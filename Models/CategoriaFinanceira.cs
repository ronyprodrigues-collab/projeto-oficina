using System;
using System.Collections.Generic;

namespace Models
{
    public class CategoriaFinanceira : ISoftDeletable
    {
        public int Id { get; set; }
        public int OficinaId { get; set; }
        public Oficina Oficina { get; set; } = null!;
        public string Nome { get; set; } = string.Empty;
        public FinanceiroTipoLancamento Tipo { get; set; } = FinanceiroTipoLancamento.Despesa;
        public string? Descricao { get; set; }
        public bool Ativo { get; set; } = true;

        public ICollection<LancamentoFinanceiro> Lancamentos { get; set; } = new List<LancamentoFinanceiro>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
