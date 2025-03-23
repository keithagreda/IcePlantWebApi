using Domain.ApiResponse;
using POSIMSWebApi.Application.Dtos.InventoryReconcillation;

namespace POSIMSWebApi.Application.Interfaces
{
    public interface IInventoryReconcillationService
    {
        Task<ApiResponse<string>> Create(CreateInventoryReconcillationDto input);
    }
}