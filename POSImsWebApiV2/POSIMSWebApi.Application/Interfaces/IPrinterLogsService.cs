using Domain.ApiResponse;

namespace POSIMSWebApi.Application.Interfaces
{
    public interface IPrinterLogsService
    {
        Task<ApiResponse<bool>> CreatePrinterLogs(string transNum);
        Task<ApiResponse<int>> GetPrinterLogs(string transNum);
    }
}