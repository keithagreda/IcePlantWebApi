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
        public async Task<ActionResult<ApiResponse<GetMachineGenerationWTotal>>> GetAllMachineGeneration([FromQuery] GetMachineGenerationInput input)
        {
            try
            {
                var query = _unitOfWork.MachineProduction.GetQueryable()
               .Include(e => e.MachineFk)
               .Include(e => e.ProductFk);

                var count = await query.CountAsync();

                var machineGeneration = await query
                    .WhereIf(input.MinCreationTime is not null, e => e.CreationTime >= input.MinCreationTime)
                    .WhereIf(input.MaxCreationTime is not null, e => e.CreationTime <= input.MaxCreationTime)
                    .GroupBy(e => e.MachineFk.Description)
                    .Select(machineGroup => new GetMachineGenerationV1Dto
                    {
                        MachineName = machineGroup.Key, // Machine Name
                        Good = machineGroup.Where(e => e.ProductFk.Name == "Ice Block").Select(e => e.Quantity).Sum(),
                        Bad = machineGroup.Where(e => e.ProductFk.Name == "Reject").Select(e => e.Quantity).Sum()
                    }).ToListAsync();

                var totalGood = Math.Round(machineGeneration.Sum(e => e.Good),2, MidpointRounding.AwayFromZero);
                var totalBad = Math.Round(machineGeneration.Sum(e => e.Bad),2, MidpointRounding.AwayFromZero);


                var res = new GetMachineGenerationWTotal 
                { 
                    TotalGood = machineGeneration.Sum(e => e.Good),
                    TotalGoodPercentage = Math.Round((totalGood / (totalGood + totalBad)) * 100, 2, MidpointRounding.AwayFromZero) ,
                    GetMachineGenerationV1Dtos = machineGeneration,
                };


                return ApiResponse<GetMachineGenerationWTotal>.Success(res);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
