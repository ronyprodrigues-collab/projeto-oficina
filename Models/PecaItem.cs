using System;

namespace Models
{
    public class PecaItem : ISoftDeletable
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
        public int Quantidade { get; set; }
        public bool Concluido { get; set; }
        public int? PecaEstoqueId { get; set; }
        public PecaEstoque? PecaEstoque { get; set; }

        public int OrdemServicoId { get; set; }
        public OrdemServico OrdemServico { get; set; } = null!;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
