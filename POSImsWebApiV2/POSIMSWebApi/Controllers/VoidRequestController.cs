using Domain.ApiResponse;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Pagination;
using POSIMSWebApi.Application.Dtos.VoidRequest;
using POSIMSWebApi.Authentication;
using POSIMSWebApi.QueryExtensions;

namespace POSIMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoidRequestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AuthContext _authContext;

        public VoidRequestController(IUnitOfWork unitOfWork, AuthContext authContext)
        {
            _unitOfWork = unitOfWork;
            _authContext = authContext;
        }

        //public async Task<ActionResult<ApiResponse<PaginatedResult<GetVoidRequest>>> GetVoidRequest(GetVoidRequestInput input)
        //{
        //    var voidReq = _unitOfWork.VoidRequest.GetQueryable().ToList();
        //    var user = _authContext.Users.ToList();


        //    var join = from vr in voidReq
        //               join u in user on vr.ApproverId equals u.Id
        //               select new GetVoidRequest
        //               {
                           
        //               }





        //}
    }
}
