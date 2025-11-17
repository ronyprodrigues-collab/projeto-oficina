namespace Models.ViewModels
{
    public class PlanoRestritoViewModel
    {
        public string Modulo { get; set; } = "Este módulo";
        public PlanoConta PlanoNecessario { get; set; } = PlanoConta.Pro;
        public PlanoConta PlanoAtual { get; set; } = PlanoConta.Basico;
    }
}
