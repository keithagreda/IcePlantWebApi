using Domain.ApiResponse;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Pagination;
using POSIMSWebApi.Application.Dtos.VoidRequest;
using POSIMSWebApi.Application.Services;
using POSIMSWebApi.Authentication;
using POSIMSWebApi.QueryExtensions;
using System.Security.Claims;

namespace POSIMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoidRequestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AuthContext _authContext;
        private readonly IVoidRequestService _voidRequestService;

        public VoidRequestController(IUnitOfWork unitOfWork, AuthContext authContext, IVoidRequestService voidRequestService)
        {
            _unitOfWork = unitOfWork;
            _authContext = authContext;
            _voidRequestService = voidRequestService;
        }
        [Authorize]
        [HttpGet("GetVoidRequest")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<GetVoidRequest>>>> GetVoidRequest(GetVoidRequestInput input)
        {
            var voidReq = _unitOfWork.VoidRequest.GetQueryable()
                .Include(e => e.SalesHeaderFk)
                .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => false || e.SalesHeaderFk.TransNum == input.FilterText);

            var user = _authContext.Users.Select(e => new
            {
                e.Id,
                e.UserName
            }).ToList();

            var count = await voidReq.CountAsync();

            var paginated = await voidReq.Select(e => new GetVoidRequest
            {
                ApproverName = user.FirstOrDefault(u => u.Id == e.ApproverId.ToString()).UserName,
                TransNum = e.SalesHeaderFk.TransNum,
                VoidRequestDto = new VoidRequestDto
                {
                    Status = e.Status,
                    ApproverId = e.ApproverId,
                    Id = e.Id,
                    SalesHeaderId = e.SalesHeaderId
                }
            }).ToPaginatedResult(input.PageNumber, input.PageSize).ToListAsync();

            var result = new PaginatedResult<GetVoidRequest>(paginated, count, (int)input.PageNumber, (int)input.PageSize);


            return Ok(ApiResponse<PaginatedResult<GetVoidRequest>>.Success(result));
        }
        [Authorize]

        [HttpPost("CreateVoidRequest")]
        public async Task<ActionResult<ApiResponse<string>>> CreateVoidRequest(Guid salesHeaderId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _voidRequestService.CreateVoidRequest(salesHeaderId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        [Authorize]
        [HttpPost("UpdateVoidRequest")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateVoidRequest(Guid voidReqId, VoidRequestStatus status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _voidRequestService.UpdateVoidRequest(voidReqId, status, userId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
