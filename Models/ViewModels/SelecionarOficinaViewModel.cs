using System.Collections.Generic;

namespace Models.ViewModels
{
    public class SelecionarOficinaViewModel
    {
        public List<GrupoOficinaDisponivelViewModel> Grupos { get; set; } = new();
        public string? ReturnUrl { get; set; }
        public int GrupoSelecionado { get; set; }
        public int OficinaSelecionada { get; set; }
    }

    public class GrupoOficinaDisponivelViewModel
    {
        public int GrupoId { get; set; }
        public string GrupoNome { get; set; } = string.Empty;
        public string Plano { get; set; } = string.Empty;
        public int OficinasUsadas { get; set; }
        public int? OficinasPermitidas { get; set; }
        public bool PodeCriarNovas { get; set; } = true;
        public List<OficinaDisponivelViewModel> Oficinas { get; set; } = new();
    }

    public class OficinaDisponivelViewModel
    {
        public int OficinaId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
