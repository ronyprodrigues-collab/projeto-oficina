using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Models.ViewModels
{
    public class ImportClientesViewModel
    {
        [Display(Name = "Arquivo CSV")]
        public IFormFile? Arquivo { get; set; }

        public bool Processado { get; set; }
        public int TotalImportados { get; set; }
        public IList<string> Erros { get; set; } = new List<string>();
    }
}
