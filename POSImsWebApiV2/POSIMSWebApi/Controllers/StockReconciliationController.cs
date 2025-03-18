using Domain.ApiResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POSIMSWebApi.Application.Dtos.StocksReconciliation;
using POSIMSWebApi.Application.Interfaces;

namespace POSIMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockReconciliationController : ControllerBase
    {
        private IStocksReconciliationService _stocksReconciliationService;
        public StockReconciliationController(IStocksReconciliationService stocksReconciliationService)
        {
            _stocksReconciliationService = stocksReconciliationService;
        }
        [Authorize]
        [HttpPost("CreateOrEditStocksReconciliation")]
        public async Task<ActionResult<ApiResponse<string>>> CreateOrEditStocksReconciliation(CreateOrEditStocksReconciliationDto input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(await _stocksReconciliationService.CreateStocksReconciliation(input));
        }
    }
}
