using Domain.ApiResponse;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.InventoryReconcillation;
using POSIMSWebApi.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Services
{
    public class InventoryReconcillationService : IInventoryReconcillationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        public InventoryReconcillationService(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<ApiResponse<string>> Create(CreateInventoryReconcillationDto input)
        {
            var getCurrentOpenedInventory =  await _unitOfWork.InventoryBeginning.GetQueryable().FirstOrDefaultAsync(e => e.Status == Domain.Enums.InventoryStatus.Open);
            if (getCurrentOpenedInventory is null) return ApiResponse<string>.Fail("Invalid Action! There is no active inventory right now.");
            var invRecon = new InventoryReconciliation
            { 
                ProductId = input.ProductId,
                InventoryBeginningId = getCurrentOpenedInventory.Id,
                //RemarksId = input.RemarksId,
                Quantity = input.Quantity,
            };

            await _unitOfWork.InventoryReconciliation.AddAsync(invRecon);
            await _unitOfWork.CompleteAsync();
            _cacheService.RemoveInventoryCache();
            return ApiResponse<string>.Success("Successfully saved reconcillation!");
        }

    }
}
