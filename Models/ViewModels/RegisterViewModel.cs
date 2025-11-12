using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        public string Cargo { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare("Senha")] 
        public string ConfirmarSenha { get; set; } = string.Empty;
    }
}

