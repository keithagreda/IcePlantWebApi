using Domain.ApiResponse;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Machine;
using POSIMSWebApi.Authentication;

namespace POSIMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public MachineController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet("GetAllMachine")]
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Cashier)]
        public async Task<ActionResult<ApiResponse<List<MachineDto>>>> GetAllMachine()
        {
            var query = await _unitOfWork.Machine.GetQueryable().Select(e => new MachineDto
            {
                Description = e.Description,
                Id = e.Id
            }).ToListAsync();

            if (!query.Any())
            {
                return ApiResponse<List<MachineDto>>.Success([]);
            }
            return ApiResponse<List<MachineDto>>.Success(query);
        }
        [HttpPost("CreateOrEditMachineDto")]
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Cashier)]
        public async Task<ActionResult<ApiResponse<string>>> CreateOrEdit([FromBody]CreateOrEditMachineDto input)
        {
            if(input.Id is null)
            {
                return Ok(await Create(input));
            }
            return Ok(await Edit(input));
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Cashier)]
        private async Task<ApiResponse<string>> Create(CreateOrEditMachineDto input)
        {
            try
            {
                var newMachine = new Machine
                {
                    Description = input.Description
                };

                await _unitOfWork.Machine.AddAsync(newMachine);
                await _unitOfWork.CompleteAsync();
                return ApiResponse<string>.Success("Successfully Created A Machine");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Cashier)]
        private async Task<ApiResponse<string>> Edit(CreateOrEditMachineDto input)
        {
            var existingMachine = await _unitOfWork.Machine.FirstOrDefaultAsync(e => e.Id == input.Id);
            if(existingMachine is null)
            {
                return ApiResponse<string>.Fail("Error! Machine Not Found");
            }

            existingMachine.Description = input.Description;
            await _unitOfWork.CompleteAsync();
            return ApiResponse<string>.Success("Successfully updated!");
        }
        [HttpGet("GetForEdit/{id}")]
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Cashier)]
        public async Task<ActionResult<ApiResponse<CreateOrEditMachineDto>>> GetForEdit(int id)
        {
            var dto = await _unitOfWork.Machine.GetQueryable().Select(e => new CreateOrEditMachineDto
            {
                Id = e.Id,
                Description = e.Description
            }).FirstOrDefaultAsync();

            if (dto is null)
            {
                return Ok(ApiResponse<CreateOrEditMachineDto>.Fail("Error! Machine Not Found"));
            }
            return Ok(ApiResponse<CreateOrEditMachineDto>.Success(dto));
        }
    }
}
