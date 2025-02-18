using Domain.ApiResponse;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Services
{
    public class MachineProductionService : IMachineProductionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public MachineProductionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<string>> CreateOrEdit(int machineId, Guid stocksReceivingId)
        {
            return await Create(machineId, stocksReceivingId);
        }

        private async Task<ApiResponse<string>> Create(int machineId, Guid stocksReceivingId)
        {
            var newProdction = new MachineProduction
            {
                MachineId = machineId,
                StocksReceivingId = stocksReceivingId
            };
            await _unitOfWork.MachineProduction.AddAsync(newProdction);
            await _unitOfWork.CompleteAsync();
            return ApiResponse<string>.Success("Success!");
        }


    }
}
