using System;

namespace Models
{
    public class OficinaCliente
    {
        public int OficinaId { get; set; }
        public Oficina Oficina { get; set; } = null!;
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;
        public DateTime VinculadoEm { get; set; } = DateTime.UtcNow;
        public string? Observacao { get; set; }
    }
}
