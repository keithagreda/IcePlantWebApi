using Domain.ApiResponse;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POSIMSWebApi.Application.Dtos.Report;
using POSIMSWebApi.Application.Interfaces;

namespace POSIMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IUnitOfWork _unitOfWork;
        public ReportController(IReportService reportService, IUnitOfWork unitOfWork)
        {
            _reportService = reportService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GenerateReport")]
        public async Task<ActionResult<ApiResponse<ViewGeneratedReportDto>>> GenerateReport(DateTime date)
        {
            return Ok(await _reportService.GenerateReport(date));
        }
    }
}
