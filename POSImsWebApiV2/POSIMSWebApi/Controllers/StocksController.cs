using Domain.ApiResponse;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Pagination;
using POSIMSWebApi.Application.Dtos.ProductDtos;
using POSIMSWebApi.Application.Dtos.Stocks;
using POSIMSWebApi.Application.Dtos.StocksReceiving;
using POSIMSWebApi.Application.Interfaces;
using POSIMSWebApi.Authentication;
using POSIMSWebApi.QueryExtensions;

namespace POSIMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockReceivingService _stockReceivingService;
        public StocksController(IUnitOfWork unitOfWork, IStockReceivingService stockReceivingService)
        {
            _unitOfWork = unitOfWork;
            _stockReceivingService = stockReceivingService;
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Inventory)]
        [HttpPost("ReceiveStocks")]
        public async Task<ActionResult<ApiResponse<string>>> ReceiveStocks(CreateStocksReceivingDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _stockReceivingService.ReceiveStocks(input);
            _unitOfWork.Complete();

            return Ok(result);

           // return result.Match<IActionResult>(
           //success => CreatedAtAction(nameof(ReceiveStocks), new { id = input.ProductId, input.Quantity, input.StorageLocationId }, success),
           //error => BadRequest(error));
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Inventory)]
        [HttpGet("GetReceivingStocks")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<GetAllStocksReceivingDto>>>> GetReceiveStocks([FromQuery]GenericSearchParamsWithDate input)
        {

            var query = _unitOfWork.StocksReceiving.GetQueryable()
                .Include(e => e.StocksHeaderFk).ThenInclude(e => e.ProductFK)
                .Include(e => e.StocksHeaderFk.StorageLocationFk)
                .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => false
                || e.StocksHeaderFk.ProductFK.Name.Contains(input.FilterText)
                || e.TransNum.Contains(input.FilterText))
                .WhereIf(input.Date is not null, e => e.CreationTime.Date == input.Date.GetValueOrDefault().Date);

            var count = await query.CountAsync();

            var result = await query
                .OrderByDescending(e => e.CreationTime)
                .Select(e => new GetAllStocksReceivingDto
                {
                    ProductId = e.StocksHeaderFk.ProductId,
                    ProductName = e.StocksHeaderFk.ProductFK.Name,
                    TransNum = e.TransNum,
                    Quantity = e.Quantity,
                    StorageLocation = e.StocksHeaderFk != null && e.StocksHeaderFk.StorageLocationFk != null
                    ? e.StocksHeaderFk.StorageLocationFk.Name
                    : "N/A",
                    StorageLocationId = e.StocksHeaderFk != null && e.StocksHeaderFk.StorageLocationId.HasValue
                    ? e.StocksHeaderFk.StorageLocationId.Value
                    : 0,
                    DateReceived = e.CreationTime.ToString("f"),
                    Id = e.Id
                })
                .ToPaginatedResult(input.PageNumber, input.PageSize)
                .ToListAsync();

            return Ok(ApiResponse<PaginatedResult<GetAllStocksReceivingDto>>.Success(
                new PaginatedResult<GetAllStocksReceivingDto>(result, count, (int)input.PageNumber, (int)input.PageSize)
                ));
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Inventory + "," + UserRole.Owner)]
        [HttpGet("GetStocksGeneration")]
        public async Task<ActionResult<ApiResponse<List<GetStocksGenerationDto>>>> GetStocksGeneration([FromQuery]GetStocksGenerationInput input)
        {
            var res = await _stockReceivingService.GetStocksGeneration(input);
            return Ok(res);
        }
    }
}
