using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public decimal PercentualComissao { get; set; }
        public ICollection<OficinaUsuario> Oficinas { get; set; } = new List<OficinaUsuario>();
        public ICollection<GrupoOficina> GruposDiretor { get; set; } = new List<GrupoOficina>();
        public ICollection<GrupoOficina> GruposAdministrador { get; set; } = new List<GrupoOficina>();
    }
}
