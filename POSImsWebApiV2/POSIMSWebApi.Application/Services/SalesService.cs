﻿using Domain.ApiResponse;
using Domain.Entities;
using Domain.Error;
using Domain.Interfaces;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using POSIMSWebApi.Application.Dtos.Inventory;
using POSIMSWebApi.Application.Dtos.Pagination;
using POSIMSWebApi.Application.Dtos.ProductCostDetail;
using POSIMSWebApi.Application.Dtos.ProductDtos;
using POSIMSWebApi.Application.Dtos.Sales;
using POSIMSWebApi.Application.Dtos.Stocks;
using POSIMSWebApi.Application.Interfaces;
using POSIMSWebApi.QueryExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace POSIMSWebApi.Application.Services
{
    public class SalesService : ISalesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IInventoryService _inventoryService;
        private readonly IMemoryCache _memoryCache;
        private static readonly SemaphoreSlim sempahore = new SemaphoreSlim(1, 1);
        private readonly string _totalSalesKey = "TotalSales";
        private readonly string _salesGraphKey = "SalesGraph";

        public SalesService(IUnitOfWork unitOfWork, ICacheService cacheService, IInventoryService inventoryService, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _inventoryService = inventoryService;
            _memoryCache = memoryCache;
        }



        /// <summary>
        /// FOR SINGLE TRANSNUM FIRST
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ApiResponse<string>> CreateSalesFromTransNum(CreateOrEditSalesDto input)
        {
            try
            {
                //access product stock details
                var stocksReceiving = _unitOfWork.StocksReceiving.GetQueryable()
                    .Include(e => e.StocksHeaderFk);

                var transNumReaderDto = input.CreateSalesDetailDtos.Select(e => e.TransNumReaderDto).ToList();

                //projection to get details product id and loc
                var productStockDetails = await stocksReceiving.Where(e => transNumReaderDto.Select(e => e.TransNum).Contains(e.TransNum)).Select(e => new
                {
                    e.TransNum,
                    e.StocksHeaderFk.ProductId,
                    e.StocksHeaderFk.StorageLocationId,
                }).ToListAsync();

                var transDetails = transNumReaderDto.Select(e => new TransDetails
                {
                    ProductId = productStockDetails.FirstOrDefault(p => p.TransNum == e.TransNum)?.ProductId ?? 0,
                    StorageLocationId = productStockDetails.FirstOrDefault(s => s.TransNum == e.TransNum)?.StorageLocationId ?? 0,
                    Quantity = e.Quantity,
                    TransNum = e.TransNum
                }).ToList();

                //Validation for product details
                if (productStockDetails.Count <= 0)
                {
                    var error = new ArgumentNullException(nameof(input.CreateSalesDetailDtos));
                    return ApiResponse<string>.Fail(error.ToString());
                }

                var product = await _unitOfWork.Product.GetQueryable().Where(e => transDetails.Select(e => e.ProductId).Contains(e.Id)).Select(e => new CreateProductSales
                {
                    Id = e.Id,
                    Name = e.Name,
                    Price = e.Price
                }).ToListAsync();

                if (product.Count <= 0)
                {
                    var error = new ValidationException("Error! Product not found!");
                    return ApiResponse<string>.Fail(error.ToString());
                }

                var salesHeader = new SalesHeader()
                {
                    Id = Guid.NewGuid(),
                    TotalAmount = 0,
                    TransNum = await GenerateTransNum()
                };

                if (input.CustomerId is not null)
                {
                    var customer = await _unitOfWork.Customer.FirstOrDefaultAsync(e => e.Id == input.CustomerId);

                    if (customer is null)
                    {
                        var error = new ValidationException("Error! Customer not found.");
                        return ApiResponse<string>.Fail(error.ToString());
                    }

                    salesHeader.CustomerId = customer.Id;
                }

                var resGetStocks = new List<GetStockDetailsDto>();
                var saleDetails = new List<SalesDetail>();
                //TO DO FIGURE OUT HOW TO DEDUCT QTY IF STOCKS ARE NOT ENOUGH
                foreach (var item in transDetails)
                {
                    //to deduct items from stocks
                    var res = await stocksReceiving.Include(e => e.StocksHeaderFk.StocksDetails)
                        .Where(e => e.StocksHeaderFk.ProductId == item.ProductId
                        && e.StocksHeaderFk.StorageLocationId == item.StorageLocationId)
                        .OrderByDescending(e => e.StocksHeaderFk.ExpirationDate)
                        .Select(e => new GetStockDetailsDto
                        {
                            ProductId = e.StocksHeaderFk.ProductId,
                            StorageLocationId = e.StocksHeaderFk.StorageLocationId != null ? (int)e.StocksHeaderFk.StorageLocationId : 0,
                            OverallStock = e.StocksHeaderFk.StocksDetails.Count(),
                            StocksDetails = new List<StocksDetail>(e.StocksHeaderFk.StocksDetails
                            .Where(e => e.Unavailable == false).Take(item.Quantity))
                        }).FirstOrDefaultAsync();
                    if (res is null)
                    {
                        var error = new ArgumentNullException("Error! A Product can't be found...");
                        return ApiResponse<string>.Fail(error.ToString());
                    }
                    resGetStocks.Add(res);
                    //to create stock details
                    var currAmount = CalculateAmount(product, item.ProductId, item.Quantity);
                    var saleDetail = new SalesDetail
                    {
                        Id = Guid.NewGuid(),
                        ActualSellingPrice = 0, //TEMPORARY
                        Amount = currAmount,
                        Quantity = item.Quantity,
                        ProductPrice = product != null ? product.FirstOrDefault().Price : 0,
                        ProductId = item.ProductId,
                        Discount = 0, // TODO: Temporary CalculateDiscount(input.CreateSalesDetailDtos.ActualSellingPrice, currAmount),
                        SalesHeaderId = salesHeader.Id
                    };
                    saleDetails.Add(saleDetail);
                }

                var stocksToBeDeducted = resGetStocks.SelectMany(e => e.StocksDetails);
                var stocksDeductedCount = await _unitOfWork.StocksDetail.UpdateRangeAsync(stocksToBeDeducted, null, stockDetail =>
                {
                    stockDetail.Unavailable = true;
                });

                await _unitOfWork.SalesHeader.AddAsync(salesHeader);
                await _unitOfWork.SalesDetail.AddRangeAsync(saleDetails);
                _unitOfWork.Complete();


                return ApiResponse<string>.Success("Success!");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// for manual encoding
        ///  doesnt transact with stock details
        ///  doesnt utilize storage location since the storage is already in place for FIFO system
        /// </summary>
        /// <param name="input"></param>
        /// <returns>string</returns>
        public async Task<ApiResponse<string>> CreateSales(CreateOrEditSalesV1Dto input)
        {
            try
            {
                //access product stock details
                var stocksReceiving = _unitOfWork.StocksReceiving.GetQueryable()
                    .Include(e => e.StocksHeaderFk);

                var transDetails = input.CreateSalesDetailV1Dto.ToList();

                //Validation for product details
                if (transDetails.Count <= 0)
                {
                    var error = new ArgumentNullException("Products can't be null", nameof(input.CreateSalesDetailV1Dto));
                    return ApiResponse<string>.Fail(error.ToString());
                }


                var transJoin = (from t in transDetails
                                 join p in _unitOfWork.Product.GetQueryable()
                                 on t.ProductId equals p.Id
                                 select new CreateProductSales
                                 {
                                     Id = p.Id,
                                     ActualSellingPrice = t.ActualSellingPrice != null ? (decimal)t.ActualSellingPrice : 0,
                                     Name = p.Name,
                                     Price = p.Price,
                                     Quantity = t.Quantity,
                                 }).ToList();


                if (transJoin.Count <= 0)
                {
                    var error = new ValidationException("Error! Product not found!");
                    return ApiResponse<string>.Fail(error.ToString());
                }

                var salesHeader = new SalesHeader()
                {
                    Id = Guid.NewGuid(),
                    TotalAmount = 0,
                    TransNum = await GenerateTransNum(),
                    InventoryBeginningId = await _inventoryService.CreateOrGetInventoryBeginning()
                };
                Guid? customerId = null;
                //create customer
                if (!string.IsNullOrEmpty(input.CustomerName))
                {
                    var existingCustomer = await _unitOfWork.Customer.GetQueryable().FirstOrDefaultAsync(e => e.Name.Contains(input.CustomerName));
                    if (existingCustomer is null)
                    {
                        var customer = new Customer
                        {
                            Name = input.CustomerName,
                        };
                        customerId = await _unitOfWork.Customer.InsertAndGetGuidAsync(customer);
                    }
                    //lookup if existing
                    if (existingCustomer is not null)
                    {
                        customerId = existingCustomer.Id;
                    }

                    salesHeader.CustomerId = customerId;
                }

                var saleDetails = new List<SalesDetail>();
                
                //TO DO FIGURE OUT HOW TO DEDUCT QTY IF STOCKS ARE NOT ENOUGH
                foreach (var item in transJoin)
                {
                    //to deduct items from stocks
                    //since its first in first out 
                    //to create stock details
                    var currAmount = CalculateAmount(transJoin, item.Id, item.Quantity);

                    var saleDetail = new SalesDetail
                    {
                        Id = Guid.NewGuid(),
                        ActualSellingPrice = item.ActualSellingPrice != 0 ? item.ActualSellingPrice : 0m, //
                        Amount = currAmount,
                        Quantity = item.Quantity,
                        ProductPrice = item.Price,
                        ProductId = item.Id,
                        Discount = item.ActualSellingPrice != 0 ? (currAmount - item.ActualSellingPrice) / currAmount * 100 : 0m, // 
                        SalesHeaderId = salesHeader.Id
                    };

                    //create productCostDetail

                    salesHeader.TotalAmount = salesHeader.TotalAmount += currAmount;
                    saleDetails.Add(saleDetail);
                }

                //make a notification when discount is more than 30%

                await _unitOfWork.SalesHeader.AddAsync(salesHeader);
                await _unitOfWork.SalesDetail.AddRangeAsync(saleDetails);
                await _unitOfWork.CompleteAsync();
                _cacheService.RemoveInventoryCache();
                _memoryCache.Remove(_totalSalesKey);
                _memoryCache.Remove(_salesGraphKey);
                return ApiResponse<string>.Success("Success!");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail(ex.Message);
            }
        }
        /// <summary>
        /// re calculate sales header and set sales header to is reconciled;
        /// </summary>
        /// <returns></returns>
        //public async Task<ApiResponse<>> ReconcileSalesHeader(Guid salesDetailId)
        //{
         
        //}

        private async Task<string> GenerateTransNum()
        {
            var currentDate = DateTime.UtcNow;
            //SGT
            var shortDate = currentDate.AddHours(8).ToString("yyyyMMdd");
            var salesHeaderCount = await _unitOfWork.SalesHeader.GetQueryable()
                .IgnoreQueryFilters()
                .Where(e => e.CreationTime.Date == currentDate.Date).CountAsync() + 1;
            var formattedCount = salesHeaderCount.ToString("D4");
            var transNum = $"T{shortDate}-{formattedCount}";
            return transNum;
        }


        private decimal CalculateAmount(List<CreateProductSales> product, int productId, decimal quantity)
        {
            var productPrice = product.FirstOrDefault(e => e.Id == productId).Price;
            if (productPrice == 0)
            {
                throw new ArgumentNullException("Error! Product Price not found", nameof(productPrice));
            }
            return productPrice * quantity;
        }

        private decimal CalculateDiscount(decimal asp, decimal amt)
        {
            if (asp == 0)
            {
                return 0;
            }

            decimal discountAmount = amt - asp;
            decimal disPercentage = (discountAmount / asp) * 100;
            return disPercentage;
        }

        private decimal SalesHelper(decimal productPrice, decimal quantity, decimal? actualSellingPrice)
        {
            if (actualSellingPrice != 0)
            {
                return (decimal)actualSellingPrice;
            }
            return productPrice * quantity;
        } 

        public async Task<ApiResponse<GetTotalSalesDto>> GetTotalSales()
        {
            if (!_memoryCache.TryGetValue(_totalSalesKey, out GetTotalSalesDto? result))
            {
                try
                {
                    await sempahore.WaitAsync();
                    if (!_memoryCache.TryGetValue(_totalSalesKey, out result))
                    {
                        var inventory = _unitOfWork.InventoryBeginning.GetQueryable();
                        var getCurrentInv = await inventory.FirstOrDefaultAsync(e => e.Status == Domain.Enums.InventoryStatus.Open) ?? new InventoryBeginning();
                        var previousInventories = inventory.Where(e => e.Status == Domain.Enums.InventoryStatus.Closed).OrderByDescending(e => e.CreationTime);
                        var prevInv = await previousInventories.FirstOrDefaultAsync() ?? new InventoryBeginning();

                        var sales = _unitOfWork.SalesDetail.GetQueryable().Include(e => e.SalesHeaderFk.InventoryBeginningFk);
                        var currentSales = 0m;
                        await sales.Where(e => e.SalesHeaderFk.InventoryBeginningId == getCurrentInv.Id).ForEachAsync((i) =>
                        {
                            currentSales += SalesHelper(i.ProductPrice, i.Quantity, i.ActualSellingPrice);
                        });
                        var prevInvSales = 0m;
                        await sales.Where(e => e.SalesHeaderFk.InventoryBeginningId == prevInv.Id).ForEachAsync((p) =>
                        {
                            prevInvSales += SalesHelper(p.ProductPrice, p.Quantity, p.ActualSellingPrice);
                        });

                        // Fetch previous sales in memory
                        var prevSales = await sales
                            .OrderByDescending(e => e.SalesHeaderFk.InventoryBeginningFk.CreationTime)
                            .Take(5)
                            .GroupBy(e => e.SalesHeaderFk.InventoryBeginningId)
                            .Select(g => g.Sum(e => e.ActualSellingPrice != 0 ? e.ActualSellingPrice : e.Amount))
                            .ToListAsync();


                        // Use Zip on the in-memory result
                        var perInv = prevSales
                            .Zip(prevSales.Skip(1), (prev, curr) => new
                            {
                                PrevSales = prev,
                                CurrSales = curr,
                                SalesPercentage = prev == 0 ? 0 : Convert.ToInt32(((curr - prev) / prev) * 100)
                            }).ToList();

                        if (currentSales <= 0)
                        {
                            return ApiResponse<GetTotalSalesDto>.Success(new GetTotalSalesDto
                            {
                                SalesPercentage = 0,
                                TotalSales = 0
                            });
                        };

                        var percentage = prevInvSales > 0 ? Convert.ToInt32((currentSales - prevInvSales) / prevInvSales * 100) : 0;

                        result = new GetTotalSalesDto
                        {
                            TotalSales = Convert.ToInt32(currentSales),
                            SalesPercentage = percentage,
                            AllSalesPercentage = perInv.Select(e => e.SalesPercentage).ToArray()
                        };

                        var cacheOptions = new MemoryCacheEntryOptions()
                               .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                               .SetSize(1);

                        _memoryCache.Set(_totalSalesKey, result, cacheOptions);
                    }
                }
                finally
                {
                    sempahore.Release();
                }
            }
            return ApiResponse<GetTotalSalesDto>.Success(result);
        }

        public async Task<ApiResponse<PaginatedResult<ViewSalesHeaderDto>>> ViewSales(ViewSalesParams input)
        {
            try
            {
                var query = _unitOfWork.SalesHeader.GetQueryable().Include(e => e.SalesDetails).ThenInclude(e => e.ProductFk)
                .WhereIf(input.SalesHeaderId != null, e => e.Id == input.SalesHeaderId)
                .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => e.TransNum.Contains(input.FilterText));
                var projection = await query
                    .Select(e => new ViewSalesHeaderDto
                    {
                        TransNum = e.TransNum,
                        TransDate = e.CreationTime,
                        TotalAmount = e.TotalAmount,
                        //TODO:
                        Discount = 0m,
                        ViewSalesDetailDtos = e.SalesDetails.Select(e => new ViewSalesDetailDto
                        {
                            Amount = e.ActualSellingPrice != 0 ? e.ActualSellingPrice : e.Amount,
                            ItemName = e.ProductFk.Name,
                            Quantity = e.Quantity,
                            Rate = e.ProductPrice
                        }).ToList()
                    })
                    .ToPaginatedResult(input.PageNumber, input.PageSize)
                    .OrderByDescending(e => e.TransDate)
                    .ToListAsync();

                projection.ForEach((header) =>
                {
                    var finalTotalSales = 0m;
                    header.ViewSalesDetailDtos.ForEach((item) =>
                    {
                        finalTotalSales += item.Amount;
                    });

                    if(finalTotalSales == header.TotalAmount)
                    {
                        header.FinalTotalAmount = header.TotalAmount;
                        header.Discount = 0m;
                    }
                    else
                    {
                        header.FinalTotalAmount = finalTotalSales;
                        header.Discount = Math.Round( (header.TotalAmount - finalTotalSales) / header.TotalAmount * 100, 2, MidpointRounding.AwayFromZero);
                    }
                });
                var res = new PaginatedResult<ViewSalesHeaderDto>(projection, await query.CountAsync(), (int)input.PageNumber, (int)input.PageSize);
                return ApiResponse<PaginatedResult<ViewSalesHeaderDto>>.Success(res);
            }
            catch (Exception ex)
            {

                return ApiResponse<PaginatedResult<ViewSalesHeaderDto>>.Fail(ex.Message); ;
            }
        }


        public async Task<ApiResponse<List<PerMonthSalesDto>>> GetPerMonthSales(int? year )
        {
            if (!_memoryCache.TryGetValue(_totalSalesKey, out List<PerMonthSalesDto> res))
            {
                try
                {
                    await sempahore.WaitAsync();
                    if (!_memoryCache.TryGetValue(_totalSalesKey, out res))
                    {
                        if (year is null)
                        {
                            year = DateTime.Now.Year;
                        }

                        // Create a list of all months for the specified year
                        var allMonths = Enumerable.Range(1, 12)
                            .Select(month => new { Year = year.Value, Month = month })
                            .ToList(); // Materialize as List for iteration

                        // Fetch the sales queryable
                        var sales = _unitOfWork.SalesDetail.GetQueryable().Include(e => e.SalesHeaderFk);

                        // Precompute the total sales for the year
                        var totalSales = await sales
                            .Where(sale => sale.CreationTime.Year == year)
                            .SumAsync(sale => sale.ActualSellingPrice != 0 ? sale.ActualSellingPrice : sale.Amount); // Use SumAsync for EF Core async operations

                        // Join all months with sales data
                        res = allMonths
                            .Select(month => new
                            {
                                month.Year,
                                month.Month,
                                MonthlyTotal = sales
                                    .Where(sale => sale.CreationTime.Year == month.Year && sale.CreationTime.Month == month.Month)
                                    .Sum(sale => sale.ActualSellingPrice != 0 ? sale.ActualSellingPrice : sale.Amount), // Aggregate monthly totals
                                TotalSales = totalSales,
                            })
                            .OrderBy(e => e.Month)
                            .Select(result => new PerMonthSalesDto
                            {
                                Month = new DateTime(1, result.Month, 1).ToString("MMMM"),
                                Year = result.Year.ToString(),
                                SalesPercentage = result.TotalSales == 0
                                    ? 0
                                    : (result.MonthlyTotal / result.TotalSales) * 100,
                                TotalMonthlySales = result.MonthlyTotal
                            })
                            .ToList(); // Finalize as a List

                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                            .SetSize(1);

                        _memoryCache.Set(_salesGraphKey, res, cacheOptions);
                    }
                }
                finally
                {
                    sempahore.Release();
                }
            }
            return ApiResponse<List<PerMonthSalesDto>>.Success(res);
        }
    }
}
