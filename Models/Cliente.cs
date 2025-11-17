using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Cliente : ISoftDeletable
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "CPF/CNPJ")]
        public string CPF_CNPJ { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        public string Telefone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Endereço Completo")]
        public string Endereco { get; set; } = string.Empty;

        // 🔥 Novos campos (todos opcionais para não afetar dados existentes)
        public string? Numero { get; set; }      // número da residência
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? CEP { get; set; }

        [Display(Name = "Data de Nascimento")]
        public DateTime? DataNascimento { get; set; }

        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        // 🔥 Suporte para PF e PJ
        [Display(Name = "Tipo de Cliente")]
        public string TipoCliente { get; set; } = "PF"; // PF ou PJ

        public string? CNPJ { get; set; }
        public string? Responsavel { get; set; }

        // Relacionamento com veículos
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
        public ICollection<OficinaCliente> Oficinas { get; set; } = new List<OficinaCliente>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
