using Domain.ApiResponse;
using POSIMSWebApi.Application.Dtos.Report;

namespace POSIMSWebApi.Application.Interfaces
{
    public interface IReportService
    {
        Task<ApiResponse<ViewGeneratedReportDto>> GenerateReport(DateTime date);
    }
}