namespace Models.ViewModels
{
    public class EditFuncionarioViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string? NovaSenha { get; set; }
        public bool Ativo { get; set; }
        public decimal PercentualComissao { get; set; }
    }
}
