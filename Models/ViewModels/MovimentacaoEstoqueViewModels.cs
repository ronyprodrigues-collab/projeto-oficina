using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class MovimentacaoEntradaViewModel
    {
        [Required]
        [Display(Name = "Peça")]
        public int PecaEstoqueId { get; set; }

        [Required]
        [Range(0.0001, 999999)]
        public decimal Quantidade { get; set; }

        [Required]
        [Display(Name = "Valor unitário")]
        [Range(0.01, 999999)]
        public decimal ValorUnitario { get; set; }

        [StringLength(200)]
        public string? Observacao { get; set; }
    }

    public class MovimentacaoSaidaViewModel
    {
        [Required]
        [Display(Name = "Peça")]
        public int PecaEstoqueId { get; set; }

        [Required]
        [Range(0.0001, 999999)]
        public decimal Quantidade { get; set; }

        [StringLength(200)]
        public string? Observacao { get; set; }

        public IEnumerable<PecaEstoque>? Pecas { get; set; }
    }
}
