﻿using Domain.ApiResponse;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Pagination;
using POSIMSWebApi.Application.Dtos.ProductDtos;
using POSIMSWebApi.Application.Dtos.Sales;
using POSIMSWebApi.Application.Interfaces;
using POSIMSWebApi.Application.Services;
using POSIMSWebApi.Authentication;
using POSIMSWebApi.QueryExtensions;

namespace POSIMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _salesService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        public SalesController(ISalesService salesService, IUnitOfWork unitOfWork, UserManager<ApplicationIdentityUser> userManager)
        {
            _salesService = salesService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Cashier + "," + UserRole.Owner)]
        [HttpGet("GetSales")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<SalesHeaderDto>>>> GetSales([FromQuery]FilterSales input)
        {
            //dapat naay created by
            var query = _unitOfWork.SalesHeader.GetQueryable()
                .Include(e => e.InventoryBeginningFk)
                .Include(e => e.SalesDetails)
                .ThenInclude(e => e.ProductFk)
                .Include(e => e.CustomerFk);
            var data = await query
                .OrderByDescending(e => e.CreationTime)
                .Select(e => new SalesHeaderDto
                {
                    Id = e.Id,
                    TotalAmount = e.TotalAmount,
                    TransactionDate = e.CreationTime,
                    TransNum = e.TransNum,
                    SoldBy = e.CreatedBy.ToString(),
                    CustomerName = e.CustomerFk != null ? e.CustomerFk.Name : "N/A",
                    IsInventoryClosed = e.InventoryBeginningFk.Status == Domain.Enums.InventoryStatus.Closed ? true : false,
                    SalesDetailsDto = e.SalesDetails.Select(e => new SalesDetailDto
                    {
                        ActualSellingPrice = e.ActualSellingPrice,
                        Amount = e.Amount,
                        Discount = e.Discount,
                        ProductName = e.ProductFk.Name,
                        Quantity = e.Quantity,
                        ProductPrice = e.ProductPrice
                    }).ToList()
                })
                .ToPaginatedResult(input.PageNumber, input.PageSize)
                .ToListAsync();

            foreach(var item in data)
            {
                var creator = await _userManager.FindByIdAsync(item.SoldBy);
                item.SoldBy = creator?.UserName ?? "";
            }

            var result = new PaginatedResult<SalesHeaderDto>(data, await query.CountAsync(), (int)input.PageNumber, (int)input.PageSize);

            return ApiResponse<PaginatedResult<SalesHeaderDto>>.Success(result);
                
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Cashier)]
        [HttpPost("CreateSalesFromTransNum")]
        public async Task<ActionResult<ApiResponse<string>>> CreateSalesFromTransNum(CreateOrEditSalesDto input)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var result = await _salesService.CreateSalesFromTransNum(input);
                _unitOfWork.Complete();

                return result;
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Cashier)]
        [HttpPost("CreateSales")]
        public async Task<ActionResult<ApiResponse<string>>> CreateSales(CreateOrEditSalesV1Dto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _salesService.CreateSales(input);
            _unitOfWork.Complete();
            return result;
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Owner)]
        [HttpGet("GetTotalSales")]
        public async Task<ActionResult<ApiResponse<GetTotalSalesDto>>> GetTotalSales()
        {
            var result = await _salesService.GetTotalSales();
            return Ok(result);
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Owner)]
        [HttpGet("GetTotalMonthlySales")]
        public async Task<ActionResult<ApiResponse<List<PerMonthSalesDto>>>> GetPerMonthSales(int? year)
        {
            var result = await _salesService.GetPerMonthSales(year);
            return Ok(result);
        }

        [Authorize(Roles = UserRole.Admin + "," + UserRole.Owner + "," + UserRole.Cashier)]
        [HttpGet("GetSalesSummaryForReports")]

        public async Task<ActionResult<ApiResponse<SalesSummaryWithEst>>> GetSalesSummaryForReports([FromQuery] GenericSearchParamsWithDateRange input)
        {
            //having issues with projecting estimated cost since there is no connection between what is sold and what is being received
            DateTime dateTimeFrom = DateTime.UtcNow;
            DateTime dateTimeTo = DateTime.UtcNow;

            if(input.DateFrom is not null && input.DateTo is not null)
            {
                dateTimeFrom = (DateTime)input.DateFrom;
                dateTimeTo = (DateTime)input.DateTo;    
            }

            var query = _unitOfWork.SalesDetail.GetQueryable()
                .Include(e => e.SalesHeaderFk)
                .ThenInclude(e => e.InventoryBeginningFk)
                .Include(e => e.SalesHeaderFk.CustomerFk)
                .Include(e => e.ProductFk)
                .Where(e => e.CreationTime.Date >= dateTimeFrom.Date && e.CreationTime.Date <= dateTimeTo.Date)
                .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => e.ProductFk.Name.Contains(input.FilterText))
                .Select(e => new SalesSummaryDto
                {
                    CustomerName = e.SalesHeaderFk.CustomerFk != null ? e.SalesHeaderFk.CustomerFk.Name : "-",
                    DateTime = e.CreationTime,
                    SoldBy = e.CreatedBy.ToString(),
                    ProductName = e.ProductFk.Name,
                    Quantity = e.Quantity,
                    Rate = e.ProductPrice,
                    TransNum = e.SalesHeaderFk.TransNum,
                    TotalPrice = e.ActualSellingPrice != 0 ? e.ActualSellingPrice : e.Amount,
                    CurrentInventory = e.SalesHeaderFk.InventoryBeginningFk.Status
                });

            var dailySales = await query.Where(e => e.CurrentInventory == Domain.Enums.InventoryStatus.Open).SumAsync(e => e.TotalPrice);

            var result = await query.OrderBy(e => e.DateTime).ToListAsync();
            var totalSales = result.Sum(e => e.TotalPrice);


            foreach (var item in result)
            {
                var currUser = await _userManager.FindByIdAsync(item.SoldBy);

                item.SoldBy = currUser?.UserName ?? "-";
            }

            var salesSumm = new SalesSummaryWithEst
            {
                DailySales = dailySales,
                SalesSummaryDtos = result,
                //TODO
                TotalEstimatedCost = 0,
                TotalSales = totalSales,
                From = dateTimeFrom,
                To = dateTimeTo
            };


            return Ok(ApiResponse<SalesSummaryWithEst>.Success(salesSumm));
        }
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Owner)]
        [HttpGet("GetSalesSummary")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<SalesSummaryWithEst>>>> GetSalesSummary([FromQuery] GenericSearchParams input)
        {
            //having issues with projecting estimated cost since there is no connection between what is sold and what is being received
            var query = _unitOfWork.SalesDetail.GetQueryable()
                .Include(e => e.SalesHeaderFk)
                .ThenInclude(e => e.InventoryBeginningFk)
                .Include(e => e.SalesHeaderFk.CustomerFk)
                .Include(e => e.ProductFk)
                .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => e.ProductFk.Name.Contains(input.FilterText))
                .Select(e => new SalesSummaryDto
                {
                    CustomerName = e.SalesHeaderFk.CustomerFk != null ? e.SalesHeaderFk.CustomerFk.Name : "-",
                    DateTime = e.CreationTime,
                    SoldBy =  e.CreatedBy.ToString(),
                    ProductName = e.ProductFk.Name,
                    Quantity = e.Quantity,
                    Rate = e.ProductPrice,
                    TransNum = e.SalesHeaderFk.TransNum,
                    TotalPrice = e.ActualSellingPrice != 0 ? e.ActualSellingPrice : e.Amount,
                    CurrentInventory = e.SalesHeaderFk.InventoryBeginningFk.Status
                });

            var dailySales = await query.Where(e => e.CurrentInventory == Domain.Enums.InventoryStatus.Open).SumAsync(e => e.TotalPrice);

            var result = await query.OrderByDescending(e => e.DateTime).ToPaginatedResult(input.PageNumber, input.PageSize).ToListAsync();
            var totalSales = result.Sum(e => e.TotalPrice);


            foreach (var item in result)
            {
                var currUser = await _userManager.FindByIdAsync(item.SoldBy);

                item.SoldBy = currUser?.UserName ?? "-";
            }

              var salesSumm = new SalesSummaryWithEst
                {
                    DailySales = dailySales,
                    SalesSummaryDtos = result,
                    //TODO
                    TotalEstimatedCost = 0,
                    TotalSales = totalSales,
                };

            var resWithTotal = new List<SalesSummaryWithEst>
            {
                salesSumm
            };


            var res = new PaginatedResult<SalesSummaryWithEst>(resWithTotal, await query.CountAsync(), (int)input.PageNumber, (int)input.PageSize);
            return Ok(ApiResponse<PaginatedResult<SalesSummaryWithEst>>.Success(res));
        }



        [Authorize(Roles = UserRole.Admin + "," + UserRole.Cashier + "," + UserRole.Owner)]
        [HttpGet("ViewSales")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ViewSalesHeaderDto>>>> ViewSales([FromQuery]ViewSalesParams input)
        {
            try
            {
                var query = _unitOfWork.SalesHeader.GetQueryable()
                    .Include(e => e.InventoryBeginningFk)
                    .Include(e => e.CustomerFk).Include(e => e.SalesDetails).ThenInclude(e => e.ProductFk)
                    .WhereIf(input.ThisInventory != false, e => e.InventoryBeginningFk.Status == Domain.Enums.InventoryStatus.Open)
                    .WhereIf(input.SalesHeaderId != null, e => e.Id == input.SalesHeaderId)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => e.TransNum.Contains(input.FilterText));
                    var projection = await query
                        .Select(e => new ViewSalesHeaderDto
                        {
                            TransNum = e.TransNum,
                            TransDate = e.CreationTime,
                            TotalAmount = e.TotalAmount,
                            CustomerName = e.CustomerFk.Name,
                            SoldById = e.CreatedBy,
                            SoldBy = e.CreatedBy.ToString(),
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

                foreach(var header in projection)
                {
                    var currUser = await _userManager.FindByIdAsync(header.SoldBy);

                    header.SoldBy = currUser?.UserName ?? "";

                    //var finalTotalSales = 0m;
                    //finalTotalSales = header.ViewSalesDetailDtos.Sum(e => e.Amount);

                    //foreach(var item in header.ViewSalesDetailDtos)
                    //{
                    //    finalTotalSales += item.Amount;
                    //}
                    //if (finalTotalSales == header.TotalAmount)
                    //{
                    //    header.FinalTotalAmount = header.TotalAmount;
                    //    header.Discount = 0m;
                    //}
                    //else
                    //{
                    //    header.FinalTotalAmount = finalTotalSales;
                    //    header.Discount = Math.Round((header.TotalAmount - finalTotalSales) / header.TotalAmount * 100, 2, MidpointRounding.AwayFromZero);
                    //}
                }
                var res = new PaginatedResult<ViewSalesHeaderDto>(projection, await query.CountAsync(), (int)input.PageNumber, (int)input.PageSize);
                return Ok(ApiResponse<PaginatedResult<ViewSalesHeaderDto>>.Success(res));
                //projection.ForEach(async (header) =>
                //{
                //    var currUser = await _userManager.FindByIdAsync(header.SoldBy);
                //    var finalTotalSales = 0m;
                //    header.ViewSalesDetailDtos.ForEach((item) =>
                //    {
                //        finalTotalSales += item.Amount;
                //    });


                //    if (finalTotalSales == header.TotalAmount)
                //    {
                //        header.FinalTotalAmount = header.TotalAmount;
                //        header.Discount = 0m;
                //    }
                //    else
                //    {
                //        header.FinalTotalAmount = finalTotalSales;
                //        header.Discount = Math.Round((header.TotalAmount - finalTotalSales) / header.TotalAmount * 100, 2, MidpointRounding.AwayFromZero);
                //    }
                //});

            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<PaginatedResult<ViewSalesHeaderDto>>.Fail(ex.Message));
            }
        }

        public async Task<ActionResult<ApiResponse<string>>> VoidSalesAsync(Guid id)
        {
            try
            {
                var salesToBeVoided = await _unitOfWork.SalesHeader.FirstOrDefaultAsync(e => e.Id == id);
                var salesTransNum = salesToBeVoided.TransNum;
                await _unitOfWork.SalesHeader.RemoveAsync(salesToBeVoided);
                return Ok(ApiResponse<string>.Success($"Transaction Number: {salesTransNum} has been successfully voided!"));
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

    }
}
