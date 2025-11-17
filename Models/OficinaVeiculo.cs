using System;

namespace Models
{
    public class OficinaVeiculo
    {
        public int OficinaId { get; set; }
        public Oficina Oficina { get; set; } = null!;
        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; } = null!;
        public DateTime VinculadoEm { get; set; } = DateTime.UtcNow;
        public string? Observacao { get; set; }
    }
}
