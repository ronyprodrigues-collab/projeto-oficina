using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace Models.ViewModels
{
    public class FinanceiroDashboardViewModel
    {
        public List<ContaFinanceiraResumoViewModel> Contas { get; set; } = new();
        public decimal SaldoTotal => Contas.Sum(c => c.SaldoAtual);
        public decimal TotalReceber { get; set; }
        public decimal TotalPagar { get; set; }
        public List<LancamentoResumoViewModel> PendentesReceber { get; set; } = new();
        public List<LancamentoResumoViewModel> PendentesPagar { get; set; } = new();
        public List<OrdemServicoFinanceiroViewModel> OsRecentes { get; set; } = new();
    }

    public class ContaFinanceiraResumoViewModel
    {
        public int ContaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public FinanceiroTipoConta Tipo { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
        public decimal SaldoAtual => SaldoInicial + Entradas - Saidas;
    }

    public class LancamentoResumoViewModel
    {
        public int ParcelaId { get; set; }
        public int LancamentoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public DateTime DataVencimento { get; set; }
        public decimal Valor { get; set; }
        public bool EmAtraso { get; set; }
        public FinanceiroTipoLancamento Tipo { get; set; }
    }

    public class OrdemServicoFinanceiroViewModel
    {
        public int OrdemServicoId { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime? DataConclusao { get; set; }
        public decimal ValorTotal { get; set; }
        public FinanceiroFormaPagamento FormaPagamento { get; set; }
        public string? ContaDestino { get; set; }
        public bool LancamentoGerado { get; set; }
    }

    public class LancamentoParcelaListItemViewModel
    {
        public int ParcelaId { get; set; }
        public int LancamentoId { get; set; }
        public FinanceiroTipoLancamento Tipo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public FinanceiroSituacaoParcela Situacao { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? ParceiroNome { get; set; }
    }

    public class LancamentoFinanceiroInputModel
    {
        public FinanceiroTipoLancamento Tipo { get; set; } = FinanceiroTipoLancamento.Despesa;
        public FinanceiroFormaPagamento FormaPagamento { get; set; } = FinanceiroFormaPagamento.Dinheiro;
        public int CategoriaFinanceiraId { get; set; }
        public int? ContaPadraoId { get; set; }
        public int? ClienteId { get; set; }
        public string? ParceiroNome { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string? NumeroDocumento { get; set; }
        public DateTime DataCompetencia { get; set; } = DateTime.Today;
        public DateTime DataPrimeiroVencimento { get; set; } = DateTime.Today;
        public int QuantidadeParcelas { get; set; } = 1;
        public int IntervaloDias { get; set; } = 30;
        public decimal ValorParcela { get; set; }
        public string? Observacao { get; set; }

        public Dictionary<int, string> Categorias { get; set; } = new();
        public Dictionary<int, string> Contas { get; set; } = new();
        public Dictionary<int, string> Clientes { get; set; } = new();
    }
}
