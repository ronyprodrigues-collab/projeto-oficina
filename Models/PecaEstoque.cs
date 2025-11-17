using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class PecaEstoque : ISoftDeletable
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(40)]
        public string? Codigo { get; set; }

        [Display(Name = "Unidade")]
        [Required, StringLength(10)]
        public string UnidadeMedida { get; set; } = "un";

        [Display(Name = "Estoque Mínimo")]
        [Range(0, 999999)]
        public decimal EstoqueMinimo { get; set; }

        [Display(Name = "Saldo Atual")]
        [Range(0, 999999)]
        public decimal SaldoAtual { get; set; }

        [Display(Name = "Preço de venda sugerido")]
        [Range(0, 999999)]
        public decimal? PrecoVenda { get; set; }

        public int OficinaId { get; set; }
        public Oficina Oficina { get; set; } = null!;

        public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
