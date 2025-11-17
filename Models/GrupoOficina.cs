using System;
using System.Collections.Generic;

namespace Models
{
    public class GrupoOficina : ISoftDeletable
    {
        public int Id { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string DiretorId { get; set; } = string.Empty;
        public ApplicationUser Diretor { get; set; } = null!;
        public PlanoConta Plano { get; set; } = PlanoConta.Basico;
        public string CorPrimaria { get; set; } = "#0d6efd";
        public string CorSecundaria { get; set; } = "#6c757d";
        public ICollection<Oficina> Oficinas { get; set; } = new List<Oficina>();
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
