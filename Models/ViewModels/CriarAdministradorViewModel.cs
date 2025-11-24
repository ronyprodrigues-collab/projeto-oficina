using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class CriarAdministradorViewModel
    {
        [Required]
        public int GrupoId { get; set; }

        public string GrupoNome { get; set; } = string.Empty;

        [Display(Name = "Nome completo")]
        [Required(ErrorMessage = "Informe o nome completo.")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Cargo")]
        [Required]
        public string Cargo { get; set; } = "Administrador";

        [Required, DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(Senha))]
        [Display(Name = "Confirmar senha")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }
}
