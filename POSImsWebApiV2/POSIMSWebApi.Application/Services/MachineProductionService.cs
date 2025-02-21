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

        public async Task<ApiResponse<string>> CreateOrEdit(int machineId, Guid invBegId, int productId, decimal qty, string transNum)
        {
            return await Create(machineId, invBegId, productId, qty, transNum);
        }

        private async Task<ApiResponse<string>> Create(int machineId, Guid invBegId, int productId, decimal qty, string transNum)
        {
            var newProdction = new MachineProduction
            {
                MachineId = machineId,
                ProductId = productId,
                InventoryBeginningId = invBegId,
                Quantity = qty,
                TransNum = transNum
            };
            await _unitOfWork.MachineProduction.AddAsync(newProdction);
            await _unitOfWork.CompleteAsync();
            return ApiResponse<string>.Success("Success!");
        }


    }
}
