using Domain.ApiResponse;
using Domain.Enums;
using Domain.Interfaces;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        //private readonly AuthContext _authContext;
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        private readonly IVoidRequestService _voidRequestService;

        public VoidRequestController(IUnitOfWork unitOfWork,
            //AuthContext authContext, 
            IVoidRequestService voidRequestService,
            UserManager<ApplicationIdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            //_authContext = authContext;
            _voidRequestService = voidRequestService;
            _userManager = userManager;
        }
        [Authorize]
        [HttpGet("GetVoidRequest")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<GetVoidRequest>>>> GetVoidRequest([FromQuery]GetVoidRequestInput input)
        {
            try
            {
                var voidReq = _unitOfWork.VoidRequest.GetQueryable()
               .Include(e => e.SalesHeaderFk)
               .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => false || e.SalesHeaderFk.TransNum == input.FilterText);


                var count = await voidReq.CountAsync();
                var users = await _userManager.Users.Select(e => new
                {
                    e.Id,
                    e.UserName
                }).ToListAsync();

                var paginated = await voidReq.Select(e => new GetVoidRequest
                {
                    ApproverName = "",
                    TransNum = e.SalesHeaderFk.TransNum,
                    Status = e.Status,
                    ApproverId = e.ApproverId,
                    //RequesterName = "",
                    Id = e.Id,
                    SalesHeaderId = e.SalesHeaderId,
                    CreatedBy = e.CreatedBy,
                    StrStatus = e.Status.Humanize(),
                    CreationTime = e.CreationTime
                }).ToPaginatedResult(input.PageNumber, input.PageSize).ToListAsync();

                foreach(var item in paginated)
                {
                    item.ApproverName = users.FirstOrDefault(u => u.Id == item.ApproverId)?.UserName ?? "-";
                    item.RequesterName = users.FirstOrDefault(u => u.Id == item.CreatedBy.ToString())?.UserName ?? "";
                    item.DateRequested = item.CreationTime.Humanize();
                }

                var result = new PaginatedResult<GetVoidRequest>(paginated, count, (int)input.PageNumber, (int)input.PageSize);


                return Ok(ApiResponse<PaginatedResult<GetVoidRequest>>.Success(result));
            }
            catch (Exception ex)
            {

                return BadRequest(ex);
            }
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
