﻿using Domain.ApiResponse;
using Domain.Entities;
using Domain.Error;
using Domain.Interfaces;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using POSIMSWebApi.Application.Dtos.Inventory;
using POSIMSWebApi.Application.Dtos.Stocks;
using POSIMSWebApi.Application.Dtos.StocksReceiving;
using POSIMSWebApi.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Services
{
    public class StocksReceivingService : IStockReceivingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryService _inventoryService;
        private readonly IStockDetailService _stockDetailService;
        private readonly IMachineProductionService _machineProductionService;
        private readonly IMemoryCache _memoryCache;
        private readonly string _cacheKey = "Inventory";
        private readonly string _allInventory = "AllInventory";
        private readonly ILogger<StocksReceivingService> _logger;
        public StocksReceivingService(IUnitOfWork unitOfWork,
            IStockDetailService stockDetailService,
            IInventoryService inventoryService,
            IMemoryCache memoryCache,
            IMachineProductionService machineProductionService,
            ILogger<StocksReceivingService> logger)
        {
            _unitOfWork = unitOfWork;
            _stockDetailService = stockDetailService;
            _inventoryService = inventoryService;
            _memoryCache = memoryCache;
            _machineProductionService = machineProductionService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> ReceiveStocks(CreateStocksReceivingDto input)
        {
            var currentlyOpenedInv = await _inventoryService.CreateOrGetInventoryBeginning();
            //check if inventory has beginning stocks
            var getCurrentOpenedInventory = _unitOfWork.InventoryBeginningDetails.GetQueryable().Include(e => e.InventoryBeginningFk)
                .Where(e => e.InventoryBeginningFk.Status == Domain.Enums.InventoryStatus.Open && e.ProductId == input.ProductId);

            if(!await getCurrentOpenedInventory.AnyAsync())
            {
                //dapat naay storage location
                CreateBeginningEntryDto createBeginningEntryDto = new CreateBeginningEntryDto
                {
                    ProductId = input.ProductId,
                    ReceivedQty = input.Quantity,
                };
                await _machineProductionService.CreateOrEdit(input.MachineId, currentlyOpenedInv, input.ProductId, input.Quantity, DateTime.Now.ToString("g"));
                var result = await _inventoryService.BeginningEntry(createBeginningEntryDto);
                return ApiResponse<string>.Success(result);
            }
            //have to create stocks first
            var createStocks = new CreateStocks
            {
                ProductId = input.ProductId,
                Quantity = input.Quantity,
                StorageLocationId = input.StorageLocationId
            };
            //generate transnum based on product id and current products received this day
            var transNum = TransNumGenerator(input.ProductId, input.StorageLocationId);
            //get stocks header id for receiving

            var stocksHeaderIdResult = await _stockDetailService.AutoCreateStocks(createStocks, transNum);

            if (!stocksHeaderIdResult.IsSuccess)
            {
                return ApiResponse<string>.Fail("Something went wrong while creating stocks");
            }


            // Step 5: Prepare the StocksReceiving entity
            var stocksReceiving = new StocksReceiving
            {
                StocksHeaderId = stocksHeaderIdResult.Data,
                TransNum = transNum,
                Quantity = input.Quantity,
                InventoryBeginningId = currentlyOpenedInv
            };
            

            // Step 6: Save to the database
            var stocksReceivingId = await _unitOfWork.StocksReceiving.InsertAndGetGuidAsync(stocksReceiving);

            var productCost = _unitOfWork.ProductCost.GetQueryable().Where(e => e.IsActive == true).Select(e => new
            {
                e.ProductId,
                e.Id,
                e.Amount,
            });


            var productCostDetails = new List<ProductCostDetails>();
            var currentProductCost = await productCost.Where(e => e.ProductId == input.ProductId).ToListAsync();
            if (!currentProductCost.Any())
            {
                _logger.LogWarning("Warning! No Product Cost Has Been Setup On This Product!");
            }
            if (currentProductCost.Any())
            {
                foreach (var pc in currentProductCost)
                {
                    var productCostDetail = new ProductCostDetails
                    {
                        StocksReceivingId = stocksReceivingId,
                        ProductCostId = pc.Id,
                        ProductCostTotalAmount = pc.Amount * input.Quantity
                    };
                    productCostDetails.Add(productCostDetail);

                }
                await _unitOfWork.ProductCostDetail.AddRangeAsync(productCostDetails);
            }
            
            //create machine prod
            await _machineProductionService.CreateOrEdit(input.MachineId, currentlyOpenedInv, input.ProductId, input.Quantity, transNum);

            await _unitOfWork.CompleteAsync();
            _memoryCache.Remove(_cacheKey);
            _memoryCache.Remove(_allInventory);
            return ApiResponse<string>.Success("", "Successfully received stocks!");

            // Handle potential error in stocksHeaderIdResult
            //return await stocksHeaderIdResult.Match(
            //    async stocksHeaderId =>
            //    {
            //        // Step 4: Get currently opened inventory for tagging
            //        var currentlyOpenedInv = await _unitOfWork.InventoryBeginning.CreateOrGetInventoryBeginning();

            //        // Step 5: Prepare the StocksReceiving entity
            //        var stocksReceiving = new StocksReceiving
            //        {
            //            StocksHeaderId = stocksHeaderId,
            //            TransNum = transNum,
            //            Quantity = input.Quantity,
            //            InventoryBeginningId = currentlyOpenedInv
            //        };

            //        // Step 6: Save to the database
            //        await _unitOfWork.StocksReceiving.AddAsync(stocksReceiving);
            //        return new Result<string>("Success!");
            //    },
            //    error =>
            //    {
            //        // Handle the fault case
            //        return Task.FromResult(new Result<string>(error));
            //    }
            //);
        }


        private string TransNumGenerator(int productId, int storageId)
        {
            var dateNow = DateTime.UtcNow.Date;
            var prodCode =  _unitOfWork.Product.GetQueryable().IgnoreQueryFilters().Where(e => e.Id == productId).Select(e => e.ProdCode ).FirstOrDefault();
            var stockReceiving = _unitOfWork.StocksReceiving.GetQueryable().IgnoreQueryFilters().Where(e => e.CreationTime.Date == dateNow);
            var currentTransCount = stockReceiving.Count() + 1;
            string datePart = DateTime.UtcNow.ToString("yyMMdd");
            return $"{prodCode}-{datePart}-{currentTransCount}-{storageId}";
        }

        public async Task<ApiResponse<List<GetStocksGenerationDto>>> GetStocksGeneration(GetStocksGenerationInput input)
        {
            //var receiving = _unitOfWork.StocksReceiving.GetQueryable().Include(e => e.StocksHeaderFk.ProductFK).Where(e => e.CreationTime >= e.CreationTime
            //    .AddHours(input.NumberOfHours))
            //    .GroupBy(e => e.StocksHeaderFk.ProductFK.Name)
            //    .Select(e => new GetStocksGenerationDto
            //    {
            //        ProductName = e.Key,
            //        GeneratedQuantity = e.Sum(e => e.Quantity),
            //        DifferentialPercentage
            //    });


            var receiving = _unitOfWork.StocksReceiving.GetQueryable()
                .Include(e => e.InventoryBeginningFk)
                .Include(e => e.StocksHeaderFk)
                .ThenInclude(e => e.ProductFK);

            var current = await receiving
                //.Where(e => e.CreationTime >= DateTime.UtcNow.AddHours(-input.NumberOfHours))
                .Where(e => e.InventoryBeginningFk.Status == Domain.Enums.InventoryStatus.Open)
                .GroupBy(e => e.StocksHeaderFk.ProductFK.Name)
                .Select(e => new
                {
                    ProductName = e.Key,
                    GeneratedQuantity = e.Sum(x => x.Quantity),
                }).ToListAsync();

            var baselineInventory = await receiving.Where(e => e.InventoryBeginningFk.Status == Domain.Enums.InventoryStatus.Closed).FirstOrDefaultAsync();

            if(baselineInventory is null)
            {

            }

            var baseline = await receiving
                .Where(e => e.InventoryBeginningId == baselineInventory.InventoryBeginningId)
                .GroupBy(e => e.StocksHeaderFk.ProductFK.Name)
                .Select(e => new
                {
                    ProductName = e.Key,
                    GeneratedQuantity = e.Sum(x => x.Quantity),
                }).ToListAsync();

            var differentialStockData = current
                .SelectMany(currentItem => baseline
                    .Where(baselineItem => baselineItem.ProductName == currentItem.ProductName)
                    .Select(baselineItem => new GetStocksGenerationDto
                    {
                        ProductName = currentItem.ProductName,
                        GeneratedQuantity = currentItem.GeneratedQuantity,
                        BaselineQuantity = baselineItem.GeneratedQuantity,
                        DifferentialPercentage = baselineItem.GeneratedQuantity != 0
                            ? Math.Round((currentItem.GeneratedQuantity - baselineItem.GeneratedQuantity) /
                              baselineItem.GeneratedQuantity * 100m, 2,MidpointRounding.AwayFromZero)
                            : 100m // If baseline GeneratedQuantity is 0, consider 100% difference
                    })
                    .DefaultIfEmpty(new GetStocksGenerationDto
                    {
                        ProductName = currentItem.ProductName,
                        GeneratedQuantity = currentItem.GeneratedQuantity,
                        DifferentialPercentage = 100m // If no baseline match, consider 100% difference
                    })
                ).ToList();

            return ApiResponse<List<GetStocksGenerationDto>>.Success(differentialStockData);

        }
        //public async Task<string> ReceiveStocks()
        //{
        //    var data = _unitOfWork.StocksReceiving.GetQueryable()
        //}

        //public async Task 
    }
}
