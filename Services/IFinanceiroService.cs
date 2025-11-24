using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models.ViewModels;

namespace Services
{
    public interface IFinanceiroService
    {
        Task<FinanceiroDashboardViewModel> ObterDashboardAsync(int oficinaId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LancamentoResumoViewModel>> ObterPendenciasAsync(int oficinaId, Models.FinanceiroTipoLancamento tipo, int limite = 5, CancellationToken cancellationToken = default);
    }
}
