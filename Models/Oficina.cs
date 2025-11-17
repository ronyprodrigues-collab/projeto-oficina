using System;
using System.Collections.Generic;

namespace Models
{
    public class Oficina : ISoftDeletable
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int GrupoOficinaId { get; set; }
        public GrupoOficina Grupo { get; set; } = null!;

        public PlanoConta Plano { get; set; } = PlanoConta.Basico;
        public string CorPrimaria { get; set; } = "#0d6efd";
        public string CorSecundaria { get; set; } = "#6c757d";
        public string? LogoPath { get; set; }
        public string? AdminProprietarioId { get; set; }
        public ApplicationUser? AdminProprietario { get; set; }

        public ICollection<OficinaCliente> Clientes { get; set; } = new List<OficinaCliente>();
        public ICollection<OficinaVeiculo> Veiculos { get; set; } = new List<OficinaVeiculo>();
        public ICollection<OficinaUsuario> Usuarios { get; set; } = new List<OficinaUsuario>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
