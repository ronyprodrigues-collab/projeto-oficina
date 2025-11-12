using System.Collections.Generic;

namespace Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CPF_CNPJ { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;

        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}

