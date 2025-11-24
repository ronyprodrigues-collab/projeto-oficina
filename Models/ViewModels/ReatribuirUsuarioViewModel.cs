using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.ViewModels
{
    public class ReatribuirUsuarioViewModel
    {
        [Required]
        [Display(Name = "Usuário")]
        public string? UsuarioId { get; set; }

        [Required]
        [Display(Name = "Grupo")]
        public int GrupoId { get; set; }

        [Required]
        [Display(Name = "Oficina")]
        public int OficinaId { get; set; }

        [Required]
        [Display(Name = "Perfil na oficina")]
        public string Perfil { get; set; } = "Supervisor";

        public List<SelectListItem> Usuarios { get; set; } = new();
        public List<SelectListItem> Perfis { get; set; } = new();
        public List<GrupoResumoViewModel> Grupos { get; set; } = new();
    }

    public class GrupoResumoViewModel
    {
        public int GrupoId { get; set; }
        public string GrupoNome { get; set; } = string.Empty;
        public List<OficinaResumoViewModel> Oficinas { get; set; } = new();
    }

    public class OficinaResumoViewModel
    {
        public int OficinaId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
