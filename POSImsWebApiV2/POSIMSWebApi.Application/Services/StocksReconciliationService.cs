using Domain.ApiResponse;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.StocksReconciliation;
using POSIMSWebApi.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace POSIMSWebApi.Application.Services
{
    public class StocksReconciliationService : IStocksReconciliationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRemarksService _remarksService;
        public StocksReconciliationService(IUnitOfWork unitOfWork, IRemarksService remarksService)
        {
            _unitOfWork = unitOfWork;
            _remarksService = remarksService;
        }

        public async Task<ApiResponse<string>> CreateStocksReconciliation(CreateOrEditStocksReconciliationDto input)
        {
            //get current opened inv
            var invId = await _unitOfWork.InventoryBeginning.GetQueryable().Where(e => e.Status == Domain.Enums.InventoryStatus.Open).Select(e => e.Id)
                .FirstOrDefaultAsync();
            //create remarks and save it as id then connect to this
            var remarksId = await _remarksService.CreateRemarks(input.Remarks, "RC-" + input.TransNum);
            //map to entity
            var stocksRecon = MapCoEToEntity(input, invId, remarksId);
            await _unitOfWork.StockReconciliation.AddAsync(stocksRecon);
            await _unitOfWork.CompleteAsync();
            return ApiResponse<string>.Success("Successfully created reconciliation! TransNum: " + input.TransNum);
        }

        public async Task<ApiResponse<GetReconcileProductDto>> GetTransaction(Guid id, TransactionTypeEnum type)
        {
            GetReconcileProductDto result = new GetReconcileProductDto();
            if (type == TransactionTypeEnum.Receiving)
            {
                result = await _unitOfWork.StocksReceiving.GetQueryable().Include(e => e.StocksHeaderFk).ThenInclude(e => e.ProductFK).Where(e => e.Id == id)
                    .Select(e => new GetReconcileProductDto
                    {
                        ProductId = e.StocksHeaderFk.ProductId,
                        ProductName = e.StocksHeaderFk.ProductFK.Name,
                        Quantity = e.Quantity,
                        TransNum = e.TransNum,
                        TransType = TransactionTypeEnum.Receiving
                    }).FirstOrDefaultAsync() ?? new GetReconcileProductDto();
                if(result == default)
                {
                    return ApiResponse<GetReconcileProductDto>.Fail("Error! Stocks Receiving Not Found!");
                }
                return ApiResponse<GetReconcileProductDto>.Success(result);
            }
            if (type == TransactionTypeEnum.Sales)
            {
                result = await _unitOfWork.SalesDetail.GetQueryable().Include(e => e.ProductFk).Include(e => e.SalesHeaderFk).Where(e => e.Id == id)
                    .Select(e => new GetReconcileProductDto
                    {
                        ProductId = e.ProductId,
                        ProductName = e.ProductFk.Name,
                        Quantity = e.Quantity,
                        TransNum = e.SalesHeaderFk.TransNum,
                        TransType = TransactionTypeEnum.Receiving
                    }).FirstOrDefaultAsync() ?? new GetReconcileProductDto(); ;
                if (result == default)
                {
                    return ApiResponse<GetReconcileProductDto>.Fail("Error! Sales Not Found!");
                }
                return ApiResponse<GetReconcileProductDto>.Success(result);
            }
            return ApiResponse<GetReconcileProductDto>.Fail("Error! Transaction Type Is Not Valid!");
        }
        /// <summary>
        /// purpose of this is to get current open id to save to entity
        /// </summary>
        /// <param name="input"></param>
        /// <param name="invBegId"></param>
        /// <returns></returns>
        private StockReconciliation MapCoEToEntity(CreateOrEditStocksReconciliationDto input, Guid invBegId, Guid remarksId)
        {
            return new StockReconciliation
            {
                InventoryBeginningId = (Guid)invBegId,
                TransNum = input.TransNum,
                Quantity = input.Quantity,
                RemarksId = remarksId,
            };
        }
    }
}
