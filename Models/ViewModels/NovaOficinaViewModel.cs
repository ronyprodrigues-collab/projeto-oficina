using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.ViewModels
{
    public class NovaOficinaViewModel
    {
        [Required]
        [Display(Name = "Grupo")]
        public int GrupoId { get; set; }

        [Required]
        [Display(Name = "Nome da oficina")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Display(Name = "Plano da oficina")]
        public PlanoConta Plano { get; set; } = PlanoConta.Basico;

        [Display(Name = "Administrador responsável")]
        public string? AdminId { get; set; }

        [Display(Name = "Cor primária")]
        public string? CorPrimaria { get; set; }

        [Display(Name = "Cor secundária")]
        public string? CorSecundaria { get; set; }

        public List<SelectListItem> Grupos { get; set; } = new();

        public List<SelectListItem> Admins { get; set; } = new();
    }
}
