using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Models
{
    public class Veiculo : ISoftDeletable
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }

        public int ClienteId { get; set; }
        [BindNever]
        [ValidateNever]
        public Cliente? Cliente { get; set; }

        public ICollection<OficinaVeiculo> Oficinas { get; set; } = new List<OficinaVeiculo>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
