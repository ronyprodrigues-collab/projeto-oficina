using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class OrdemServicoCreateViewModel
    {
        [Required]
        public int ClienteId { get; set; }
        [Required]
        public int VeiculoId { get; set; }
        public string? MecanicoId { get; set; }

        [Required]
        public string Descricao { get; set; } = string.Empty;
        public DateTime? DataPrevista { get; set; }
        public string? Observacoes { get; set; }

        public List<ServicoItemInput> Servicos { get; set; } = new();
        public List<PecaItemInput> Pecas { get; set; } = new();
    }

    public class ServicoItemInput
    {
        [Required]
        public string Descricao { get; set; } = string.Empty;
        [Range(0, double.MaxValue)]
        public decimal Valor { get; set; }
    }

    public class PecaItemInput
    {
        public int? PecaEstoqueId { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
        [Range(0, double.MaxValue)]
        public decimal ValorUnitario { get; set; }
        [Range(0, int.MaxValue)]
        public int Quantidade { get; set; }
    }
}

