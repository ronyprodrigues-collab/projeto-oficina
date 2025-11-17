namespace Models.ViewModels
{
    public class CriarOficinaViewModel
    {
        public int GrupoId { get; set; }
        public string GrupoNome { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? AdminId { get; set; }
    }
}
