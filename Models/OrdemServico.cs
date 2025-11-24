using System;
using System.Collections.Generic;

namespace Models
{
    public class OrdemServico : ISoftDeletable
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public int VeiculoId { get; set; }
        public string? MecanicoId { get; set; }
        public int OficinaId { get; set; }

        public string Descricao { get; set; } = string.Empty;
        public DateTime DataAbertura { get; set; }
        public DateTime? DataPrevista { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool AprovadaCliente { get; set; }
        public string? MotivoReprovacao { get; set; }
        public bool EstoqueReservado { get; set; }
        public string? Observacoes { get; set; }

        public Cliente Cliente { get; set; } = null!;
        public Veiculo Veiculo { get; set; } = null!;
        public ApplicationUser? Mecanico { get; set; }
        public Oficina Oficina { get; set; } = null!;
        public FinanceiroFormaPagamento FormaPagamento { get; set; } = FinanceiroFormaPagamento.Dinheiro;
        public int QuantidadeParcelas { get; set; } = 1;
        public DateTime? DataPrimeiroVencimento { get; set; }
        public int? ContaRecebimentoId { get; set; }
        public ContaFinanceira? ContaRecebimento { get; set; }
        public int? LancamentoFinanceiroReceitaId { get; set; }
        public int? LancamentoFinanceiroCustoPecasId { get; set; }
        public int? LancamentoFinanceiroComissaoId { get; set; }

        public List<ServicoItem> Servicos { get; set; } = new();
        public List<PecaItem> Pecas { get; set; } = new();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

