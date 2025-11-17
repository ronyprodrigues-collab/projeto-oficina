using System.Collections.Generic;
using Models;

namespace Models.ViewModels
{
    public class GrupoDiretorViewModel
    {
        public IList<GrupoOficina> Grupos { get; set; } = new List<GrupoOficina>();
        public IList<Oficina> Oficinas { get; set; } = new List<Oficina>();
    }
}
