using Domain.ApiResponse;

namespace POSIMSWebApi.Application.Services
{
    public interface IMachineProductionService
    {
        Task<ApiResponse<string>> CreateOrEdit(int machineId, Guid invBegId, int productId, decimal qty, string transNum);
    }
}