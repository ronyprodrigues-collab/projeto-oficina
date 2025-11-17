using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class MovimentacaoEstoque : ISoftDeletable
    {
        public int Id { get; set; }

        [Display(Name = "Peça")]
        public int PecaEstoqueId { get; set; }

        public PecaEstoque? PecaEstoque { get; set; }

        [Display(Name = "Data")]
        public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;

        [Required, StringLength(20)]
        public string Tipo { get; set; } = "Entrada";

        [Range(0.0001, 999999)]
        public decimal Quantidade { get; set; }

        [Range(0, 999999)]
        public decimal ValorUnitario { get; set; }

        [Range(0, 999999)]
        public decimal QuantidadeRestante { get; set; }

        [StringLength(200)]
        public string? Observacao { get; set; }

        public int? OrdemServicoId { get; set; }
        public int OficinaId { get; set; }
        public Oficina Oficina { get; set; } = null!;

        public int? MovimentacaoEntradaReferenciaId { get; set; }

        public MovimentacaoEstoque? MovimentacaoEntradaReferencia { get; set; }

        public ICollection<MovimentacaoEstoque> SaidasRelacionadas { get; set; } = new List<MovimentacaoEstoque>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
