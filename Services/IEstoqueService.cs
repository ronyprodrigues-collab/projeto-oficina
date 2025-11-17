using System.Threading;
using System.Threading.Tasks;
using Models;

namespace Services
{
    public interface IEstoqueService
    {
        Task<PecaEstoque> RegistrarEntradaAsync(int pecaId, decimal quantidade, decimal valorUnitario, string? observacao, CancellationToken cancellationToken = default);
        Task RegistrarSaidaAsync(int pecaId, decimal quantidade, string? observacao, int? ordemServicoId = null, CancellationToken cancellationToken = default);
    }
}
