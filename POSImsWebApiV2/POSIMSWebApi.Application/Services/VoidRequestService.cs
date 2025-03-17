using Domain.ApiResponse;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Pagination;
using POSIMSWebApi.Application.Dtos.VoidRequest;
using POSIMSWebApi.Application.Interfaces;
using POSIMSWebApi.QueryExtensions;

namespace POSIMSWebApi.Application.Services
{
    public class VoidRequestService : IVoidRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        public VoidRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<string>> CreateVoidRequest(Guid salesHeaderId)
        {
            var salesToBeVoided = await _unitOfWork.SalesHeader.FirstOrDefaultAsync(e => e.Id == salesHeaderId);
            if (salesToBeVoided is null)
            {
                return ApiResponse<string>.Fail("Error! Sales Not Found!");
            }

            VoidRequest voidRequest = new VoidRequest
            {
                SalesHeaderId = salesHeaderId,
                ApproverId = null,
                Status = Domain.Enums.VoidRequestStatus.Pending,
            };

            await _unitOfWork.VoidRequest.AddAsync(voidRequest);
            return ApiResponse<string>.Success("Success! Void Request has been sent!");
            //var salesTransNum = salesToBeVoided.TransNum;

            //await _unitOfWork.SalesHeader.RemoveAsync(salesToBeVoided);
            //await _unitOfWork.SalesDetail.RemoveRangeAsync(await _unitOfWork.SalesDetail.GetQueryable().Where(e => e.SalesHeaderId == salesHeaderId).ToListAsync());
        }

        public async Task<ApiResponse<string>> UpdateVoidRequest(Guid voidReqId, VoidRequestStatus status, Guid? approverId)
        {
            var voidReq = await _unitOfWork.VoidRequest.FirstOrDefaultAsync(e => e.Id == voidReqId);
            if (voidReq is null)
            {
                return ApiResponse<string>.Fail("Error! Void Request Not Found!");
            }

            if (voidReq.Status == VoidRequestStatus.Accepted || voidReq.Status == VoidRequestStatus.Declined)
            {
                return ApiResponse<string>.Fail("Invalid Action! Void Request can no longer be updated");
            }

            voidReq.Status = status;

            if(status != VoidRequestStatus.Inprogress)
            {
                voidReq.ApproverId = approverId;
            }

            if (status == VoidRequestStatus.Accepted)
            {
                var salesHeader = await _unitOfWork.SalesHeader.FirstOrDefaultAsync(e => e.Id == voidReq.SalesHeaderId);
                await _unitOfWork.SalesHeader.RemoveAsync(salesHeader);
            }
            await _unitOfWork.CompleteAsync();
            return ApiResponse<string>.Success("");
        }

    }
}
