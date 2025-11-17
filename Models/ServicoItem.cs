using System;

namespace Models
{
    public class ServicoItem : ISoftDeletable
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public bool Concluido { get; set; }

        public int OrdemServicoId { get; set; }
        public OrdemServico OrdemServico { get; set; } = null!;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
