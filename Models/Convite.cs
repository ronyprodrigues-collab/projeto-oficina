using System;

namespace Models
{
    public class Convite
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PerfilDestino { get; set; } = "Admin";
        public decimal PercentualComissao { get; set; }
        public string Token { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiraEm { get; set; }
        public bool Usado { get; set; }
    }
}
