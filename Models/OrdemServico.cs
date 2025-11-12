using System;
using System.Collections.Generic;

namespace Models
{
    public class OrdemServico
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public int VeiculoId { get; set; }
        public string? MecanicoId { get; set; }

        public string Descricao { get; set; } = string.Empty;
        public DateTime DataAbertura { get; set; }
        public DateTime? DataPrevista { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool AprovadaCliente { get; set; }
        public string? Observacoes { get; set; }

        public Cliente Cliente { get; set; } = null!;
        public Veiculo Veiculo { get; set; } = null!;
        public ApplicationUser? Mecanico { get; set; }

        public List<ServicoItem> Servicos { get; set; } = new();
        public List<PecaItem> Pecas { get; set; } = new();
    }
}

