using System.Collections.Generic;
using Models;

namespace Models.ViewModels
{
    public class SuporteConfiguracaoViewModel
    {
        public IReadOnlyList<GrupoOficina> Grupos { get; set; } = new List<GrupoOficina>();
        public int GrupoSelecionadoId { get; set; }
        public IReadOnlyList<Oficina> Oficinas { get; set; } = new List<Oficina>();
        public int OficinaSelecionadaId { get; set; }
        public Oficina? Oficina { get; set; }
        public Configuracoes? Defaults { get; set; }
    }
}
