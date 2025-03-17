using Domain.ApiResponse;
using Domain.Enums;

namespace POSIMSWebApi.Application.Services
{
    public interface IVoidRequestService
    {
        Task<ApiResponse<string>> CreateVoidRequest(Guid salesHeaderId);
        Task<ApiResponse<string>> UpdateVoidRequest(Guid voidReqId, VoidRequestStatus status, Guid? approverId);
    }
}