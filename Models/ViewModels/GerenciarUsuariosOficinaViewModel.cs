using System.Collections.Generic;

namespace Models.ViewModels
{
    public class GerenciarUsuariosOficinaViewModel
    {
        public int OficinaId { get; set; }
        public int GrupoId { get; set; }
        public string OficinaNome { get; set; } = string.Empty;
        public string GrupoNome { get; set; } = string.Empty;
        public List<UsuarioOficinaInfo> Usuarios { get; set; } = new();
        public List<UsuarioDisponivelInfo> Disponiveis { get; set; } = new();
        public string PerfilNovoUsuario { get; set; } = "Supervisor";
        public string? UsuarioSelecionadoId { get; set; }
    }

    public class UsuarioOficinaInfo
    {
        public int VínculoId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }

    public class UsuarioDisponivelInfo
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
    }
}
