using Domain.ApiResponse;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POSIMSWebApi.Application.Interfaces;

namespace POSIMSWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PrinterLogsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPrinterLogsService _printerLogsService;
        public PrinterLogsController(IUnitOfWork unitOfWork, IPrinterLogsService printerLogsService)
        {
            _unitOfWork = unitOfWork;
            _printerLogsService = printerLogsService;
        }
        [HttpPost("CreatePrinterLogs")]
        public async Task<ActionResult<ApiResponse<bool>>> CreatePrinterLogs(string transNum)
        {
            return Ok(await _printerLogsService.CreatePrinterLogs(transNum));
        }
        [HttpGet("GetPrinterLogs")]
        public async Task<ActionResult<ApiResponse<int>>> GetPrinterLogs(string transNum)
        {
            return Ok(await _printerLogsService.GetPrinterLogs(transNum));
        }
    }
}
