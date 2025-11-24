using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.ViewModels
{
    public class ConviteInputViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PerfilDestino { get; set; } = "Admin";

        [Display(Name = "Percentual de comissão (%)")]
        public decimal PercentualComissao { get; set; }

        [Display(Name = "Expira em")]
        public DateTime? ExpiraEm { get; set; }

        public SelectList? Perfis { get; set; }
    }
}
