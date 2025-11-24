namespace Models.ViewModels
{
    public class CreateFuncionarioViewModel
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Senha { get; set; } = "P@ssw0rd!";
        public decimal PercentualComissao { get; set; }
    }
}
