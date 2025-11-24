using System.Collections.Generic;
using Infrastructure.Theming;
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
        public GrupoOficina? GrupoSelecionado { get; set; }
        public IReadOnlyList<ThemePalette> Paletas { get; set; } = ThemePalettes.All;
        public ThemePalette TemaAtual { get; set; } = ThemePalettes.Default;
    }
}
