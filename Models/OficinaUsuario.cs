using System;

namespace Models
{
    public class OficinaUsuario
    {
        public int Id { get; set; }
        public int OficinaId { get; set; }
        public Oficina Oficina { get; set; } = null!;
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser Usuario { get; set; } = null!;
        public string Perfil { get; set; } = string.Empty; // Admin, Supervisor, Mecanico etc
        public bool Ativo { get; set; } = true;
        public DateTime VinculadoEm { get; set; } = DateTime.UtcNow;
    }
}
