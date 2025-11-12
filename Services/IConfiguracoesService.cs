using System.Threading;
using System.Threading.Tasks;
using Models;

namespace Services
{
    public interface IConfiguracoesService
    {
        Task<Configuracoes> GetAsync(CancellationToken cancellationToken = default);
    }
}

