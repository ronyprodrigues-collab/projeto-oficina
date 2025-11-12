namespace Models
{
    public class Configuracoes
    {
        public int Id { get; set; }
        public string? LogoPath { get; set; }
        public string CorPrimaria { get; set; } = "#0d6efd";
        public string CorSecundaria { get; set; } = "#6c757d";
        public string NomeOficina { get; set; } = "Oficina";
    }
}

