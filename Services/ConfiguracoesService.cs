using System.Threading;
using System.Threading.Tasks;
using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services
{
    public class ConfiguracoesService : IConfiguracoesService
    {
        private readonly OficinaDbContext _db;
        public ConfiguracoesService(OficinaDbContext db)
        {
            _db = db;
        }

        public async Task<Configuracoes> GetAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Configuracoes.AsNoTracking().FirstOrDefaultAsync(cancellationToken)
                   ?? new Configuracoes();
        }
    }
}

