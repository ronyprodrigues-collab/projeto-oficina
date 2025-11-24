using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class GrupoComOficinaViewModel
    {
        [Required]
        [Display(Name = "Nome do grupo")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Descrição do grupo")]
        public string? Descricao { get; set; }

        [Display(Name = "Diretor responsável")]
        public string? DiretorId { get; set; }

        [Display(Name = "Plano do grupo")]
        public PlanoConta Plano { get; set; } = PlanoConta.Basico;

        [Display(Name = "Cor primária")]
        public string CorPrimaria { get; set; } = "#0d6efd";

        [Display(Name = "Cor secundária")]
        public string CorSecundaria { get; set; } = "#6c757d";

        [Display(Name = "Criar primeira oficina agora?")]
        public bool CriarOficinaInicial { get; set; } = true;

        [Display(Name = "Nome da oficina")]
        public string? OficinaNome { get; set; }

        [Display(Name = "Descrição da oficina")]
        public string? OficinaDescricao { get; set; }

        [Display(Name = "Plano da oficina")]
        public PlanoConta OficinaPlano { get; set; } = PlanoConta.Basico;
    }
}
