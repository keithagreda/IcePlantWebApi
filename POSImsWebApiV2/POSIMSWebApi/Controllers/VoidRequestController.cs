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
using POSIMSWebApi.Application.Interfaces;
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
        public async Task<ActionResult<ApiResponse<PaginatedResult<GetVoidRequest>>>> GetVoidRequest([FromQuery] GetVoidRequestInput input)
        {
            try
            {
                var voidReq = _unitOfWork.VoidRequest.GetQueryable()
                    .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => false || e.SalesHeaderFk.TransNum == input.FilterText);

                var sales = _unitOfWork.SalesHeader.GetQueryable().IgnoreQueryFilters();

                var join = from v in voidReq
                           join s in sales on v.SalesHeaderId equals s.Id into vs
                           from s in vs.DefaultIfEmpty()
                           orderby v.CreationTime descending
                           select new GetVoidRequest
                           {
                               ApproverName = "",
                               TransNum = s.TransNum,
                               Status = v.Status,
                               ApproverId = v.ApproverId,
                               //RequesterName = "",
                               Id = v.Id,
                               SalesHeaderId = v.SalesHeaderId,
                               CreatedBy = v.CreatedBy,
                               CreationTime = v.CreationTime
                           };

                var count = await join.CountAsync();
                var users = await _userManager.Users.Select(e => new
                {
                    e.Id,
                    e.UserName
                }).ToListAsync();

                var paginated = await join
                    .ToPaginatedResult(input.PageNumber, input.PageSize)
                    .ToListAsync();

                foreach (var item in paginated)
                {
                    item.ApproverName = users.FirstOrDefault(u => u.Id == item.ApproverId)?.UserName ?? "-";
                    item.RequesterName = users.FirstOrDefault(u => u.Id == item.CreatedBy.ToString())?.UserName ?? "";
                    item.DateRequested = item.CreationTime.ToString("g");
                    item.StrStatus = item.Status.Humanize();
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
            try
            {
                var result = await _voidRequestService.CreateVoidRequest(salesHeaderId);
                return Ok(result);
            }
            catch (Exception ex)
            {

                throw ex;
            }

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
            return Ok(result);

        }
    }
}
