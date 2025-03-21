using Domain.ApiResponse;
using POSIMSWebApi.Application.Dtos.InventoryReconcillation;

namespace POSIMSWebApi.Application.Services
{
    public interface IInventoryReconcillationService
    {
        Task<ApiResponse<string>> Create(CreateInventoryReconcillationDto input);
    }
}