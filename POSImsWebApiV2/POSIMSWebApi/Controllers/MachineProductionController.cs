using Domain.ApiResponse;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.MachineProduction;
using POSIMSWebApi.Application.Dtos.Pagination;
using POSIMSWebApi.Authentication;
using POSIMSWebApi.QueryExtensions;

namespace POSIMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineProductionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        public MachineProductionController(IUnitOfWork unitOfWork, UserManager<ApplicationIdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet("GetAllMachineGeneration")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<GetMachineGenerationDto>>>> GetAllMachineGeneration([FromQuery] GetMachineGenerationInput input)
        {
            var query = _unitOfWork.MachineProduction.GetQueryable()
                .Include(e => e.MachineFk)
                .Include(e => e.StocksReceivingFk)
                .ThenInclude(e => e.StocksHeaderFk)
                .ThenInclude(e => e.ProductFK);

            var count = await query.CountAsync();

            var machineGeneration = await query
                .WhereIf(input.MinCreationTime is not null, e => e.CreationTime >= input.MinCreationTime)
                .WhereIf(input.MaxCreationTime is not null, e => e.CreationTime <= input.MaxCreationTime)
                .GroupBy(e => e.MachineFk.Description)
                .OrderBy(e => e.Select(e => e.StocksReceivingFk.StocksHeaderFk.ProductFK.Name))
                .ToPaginatedResult(input.PageNumber, input.PageSize)
                .Select(e => new GetMachineGenerationDto
                {
                    MachineName = e.Key,
                    Id = e.Select(e => e.Id).FirstOrDefault(),
                    ProductName = e.Select(e => e.StocksReceivingFk.StocksHeaderFk.ProductFK.Name).FirstOrDefault(),
                    Quantity = e.Sum(e => e.StocksReceivingFk.Quantity)
                }).ToListAsync();

            return ApiResponse<PaginatedResult<GetMachineGenerationDto>>.Success(new PaginatedResult<GetMachineGenerationDto>(machineGeneration, count, 
                (int)input.PageNumber, (int)input.PageSize));
        }
    }
}
