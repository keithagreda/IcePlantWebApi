using Domain.ApiResponse;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.InventoryReconcillation;
using POSIMSWebApi.Application.Dtos.Pagination;
using POSIMSWebApi.Application.Services;
using POSIMSWebApi.QueryExtensions;

namespace POSIMSWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryReconcillationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryReconcillationService _inventoryReconcillationService;
        public InventoryReconcillationController(IUnitOfWork unitOfWork, IInventoryReconcillationService inventoryReconcillationService)
        {
            _unitOfWork = unitOfWork;
            _inventoryReconcillationService = inventoryReconcillationService;
        }

        [HttpPost("CreateInvenReconcillation")]
        public async Task<ActionResult<ApiResponse<string>>> CreateInvenReconcillation(CreateInventoryReconcillationDto input)
        {
            var res = await _inventoryReconcillationService.Create(input);
            return res;
        }
        [HttpGet("GetAllInvenReconcillation")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<GetInventoryReconcillation>>>> GetAllInvenReconcillation(GetInventoryReconcillationInput input)
        {
            var query = _unitOfWork.InventoryReconciliation.GetQueryable()
                .Include(e => e.ProductFk)
                .Include(e => e.InventoryBeginningFk)
                .Include(e => e.RemarksFk)
                .WhereIf(!string.IsNullOrEmpty(input.FilterText), e => e.ProductFk.Name.Contains(input.FilterText));

            var count = await query.CountAsync();

            var filteredAndPaged = await query
                .OrderBy(e => e.CreationTime)
                .Select(e => new GetInventoryReconcillation
                {
                    ProductName = e.ProductFk.Name,
                    Quantity = e.Quantity,
                    IsInventoryOpen = e.InventoryBeginningFk.Status == Domain.Enums.InventoryStatus.Open ? true : false,
                    Remarks = e.RemarksFk.Description
                })
                .ToPaginatedResult(input.PageNumber, input.PageSize)
                .ToListAsync();
            return Ok(ApiResponse<PaginatedResult<GetInventoryReconcillation>>
                .Success(new PaginatedResult<GetInventoryReconcillation>(filteredAndPaged, count, (int)input.PageNumber, (int)input.PageSize)));

        }
    }
}
