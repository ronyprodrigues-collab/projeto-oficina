namespace Models
{
    public class PecaItem
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
        public int Quantidade { get; set; }

        public int OrdemServicoId { get; set; }
        public OrdemServico OrdemServico { get; set; } = null!;
    }
}

