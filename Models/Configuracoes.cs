using System;

namespace Models
{
    public class Configuracoes : ISoftDeletable
    {
        public int Id { get; set; }
        public string? LogoPath { get; set; }
        public string CorPrimaria { get; set; } = "#0d6efd";
        public string CorSecundaria { get; set; } = "#6c757d";
        public string NomeOficina { get; set; } = "Oficina";
        public PlanoConta PlanoAtual { get; set; } = PlanoConta.Pro;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
