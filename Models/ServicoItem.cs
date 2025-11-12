namespace Models
{
    public class ServicoItem
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }

        public int OrdemServicoId { get; set; }
        public OrdemServico OrdemServico { get; set; } = null!;
    }
}

