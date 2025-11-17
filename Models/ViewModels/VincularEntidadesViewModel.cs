using System.Collections.Generic;

namespace Models.ViewModels
{
    public class ClienteDisponivelViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
    }

    public class VincularClienteViewModel
    {
        public string? Busca { get; set; }
        public List<ClienteDisponivelViewModel> Clientes { get; set; } = new();
    }

    public class VeiculoDisponivelViewModel
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
    }

    public class VincularVeiculoViewModel
    {
        public string? Busca { get; set; }
        public List<VeiculoDisponivelViewModel> Veiculos { get; set; } = new();
    }
}
