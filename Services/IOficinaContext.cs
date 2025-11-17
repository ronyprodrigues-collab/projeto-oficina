using System.Threading;
using System.Threading.Tasks;
using Models;

namespace Services
{
    public interface IOficinaContext
    {
        Task<Oficina?> GetOficinaAtualAsync(CancellationToken cancellationToken = default);
        Task<bool> SetOficinaAtualAsync(int oficinaId, CancellationToken cancellationToken = default);
        void Clear();
        int? OficinaIdAtual { get; }
        string? NomeOficinaAtual { get; }
    }
}
