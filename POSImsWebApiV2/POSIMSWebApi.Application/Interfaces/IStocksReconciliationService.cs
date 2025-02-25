using Domain.ApiResponse;
using POSIMSWebApi.Application.Dtos.StocksReconciliation;

namespace POSIMSWebApi.Application.Interfaces
{
    public interface IStocksReconciliationService
    {
        Task<ApiResponse<string>> CreateStocksReconciliation(CreateOrEditStocksReconciliationDto input);
    }
}