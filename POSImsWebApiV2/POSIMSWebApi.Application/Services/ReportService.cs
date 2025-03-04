using Domain.ApiResponse;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Inventory;
using POSIMSWebApi.Application.Dtos.Report;
using POSIMSWebApi.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// this function dynamically generates a report based on the date passed
        /// but is temporarily limited to only generating monthly reports
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<ApiResponse<ViewGeneratedReportDto>> GenerateReport(DateTime date)
        {
            try
            {
                // Generate sales data query
                var salesDataQuery = await _unitOfWork.SalesDetail.GetQueryable()
                    .Include(sd => sd.SalesHeaderFk)
                    .ThenInclude(sh => sh.InventoryBeginningFk)
                    .Where(e => e.SalesHeaderFk.InventoryBeginningFk.CreationTime.Month == date.Month)
                    .GroupBy(sd => sd.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQtySold = g.Sum(sd => sd.Quantity),
                        TotalSales = g.Sum(sd => sd.Amount)
                    }).ToListAsync();

                // Fetch stock receiving data as IQueryable
                var stockReceivingDataQuery = await _unitOfWork.StocksReceiving.GetQueryable()
                    .Include(sr => sr.StocksHeaderFk)
                    .Include(e => e.InventoryBeginningFk)
                    .Where(e => e.InventoryBeginningFk.CreationTime.Month == date.Month)
                    .GroupBy(sr => sr.StocksHeaderFk.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQtyGenerated = g.Sum(sr => sr.Quantity)
                    }).ToListAsync();

                // Fetch estimated cost data
                var estimatedCost = await _unitOfWork.ProductCost.GetQueryable()
                    .Include(e => e.ProductCostDetails)
                    .ThenInclude(e => e.StocksReceivingFk)
                    .ThenInclude(e => e.InventoryBeginningFk)
                    .Where(e => e.IsActive == true && e.ProductCostDetails.Select(e => e.StocksReceivingFk.InventoryBeginningFk.CreationTime.Month).Contains(date.Month))
                    .GroupBy(e => new { e.ProductId, e.Name })
                    .Select(e => new
                    {
                        e.Key.ProductId,
                        CostName = e.Key.Name,
                        TotalCost = e.Select(e => e.ProductCostDetails.Sum(e => e.ProductCostTotalAmount)).Sum()
                    }).ToListAsync();

                var inventories = await _unitOfWork.InventoryBeginningDetails.GetQueryable()
                    .Include(e => e.InventoryBeginningFk)
                    .Include(e => e.ProductFK)
                    .Where(e => e.InventoryBeginningFk.CreationTime.Month == date.Month)
                    .GroupBy(e => e.ProductId)
                    .Select(e => new
                    {
                        e.Key,
                    }).ToListAsync();

                // Step 1: Aggregate estimated cost per product
                var estimatedCostData = estimatedCost
                    .GroupBy(e => e.ProductId)
                    .ToDictionary(
                        g => g.Key,
                        g => new ViewProdEstCosting
                        {
                            OverallTotalCost = g.Sum(e => e.TotalCost),
                            ViewProdEstCostingDetails = g.Select(e => new ViewProdEstCostingDetails
                            {
                                CostName = e.CostName,
                                TotalCost = e.TotalCost
                            }).ToList()
                        });

                var stockReceivingData = stockReceivingDataQuery
                    .ToDictionary(
                        sr => sr.ProductId,
                        sr => new ViewProdGenRecv
                        {
                            TotalQtyGenerated = sr.TotalQtyGenerated,
                            AverageGeneration = sr.TotalQtyGenerated // Modify if you need actual average calculation
                        });

                var salesData = salesDataQuery
                    .ToDictionary(
                        s => s.ProductId,
                        s => new ViewProdGenSales
                        {
                            TotalQtySold = s.TotalQtySold,
                            TotalSales = s.TotalSales
                        });

                var allProductIds = salesData.Keys
                    .Union(stockReceivingData.Keys)
                    .Union(estimatedCostData.Keys)
                    .ToList();

                var productReports = allProductIds.Select(productId => new ViewProductGeneratedReportDto
                {
                    ProductId = productId,
                    ProductName = inventories.FirstOrDefault(i => i.Key == productId)?.Key.ToString() ?? "Unknown Product", // Adjust logic based on your schema
                    Sales = salesData.ContainsKey(productId) ? salesData[productId] : null,
                    Generation = stockReceivingData.ContainsKey(productId) ? stockReceivingData[productId] : null,
                    EstCosting = estimatedCostData.ContainsKey(productId) ? estimatedCostData[productId] : null
                }).ToList();

                // Step 6: Compute report-level totals
                var result = new ViewGeneratedReportDto
                {
                    DateGenerated = DateTime.UtcNow,
                    TotalSales = productReports.Sum(p => p.Sales?.TotalSales ?? 0),
                    TotalExpenses = productReports.Sum(p => p.EstCosting?.OverallTotalCost ?? 0), // Assuming expenses = estimated cost
                    TotalEstimatedCost = productReports.Sum(p => p.EstCosting?.OverallTotalCost ?? 0), // Modify if needed
                    ViewProductGeneratedReportDtos = productReports
                };


                //// Main query: Keep sales & stock queries as subqueries
                //var query = from ibd in _unitOfWork.InventoryBeginningDetails.GetQueryable()
                //            join ib in _unitOfWork.InventoryBeginning.GetQueryable()
                //                on ibd.InventoryBeginningId equals ib.Id
                //            join p in _unitOfWork.Product.GetQueryable()
                //                on ibd.ProductId equals p.Id
                //            where ib.CreationTime.Month == date.Month
                //            group new { ibd, p } by new { ibd.ProductId } into g
                //            select new ViewProductGeneratedReportDto
                //            {
                //                ProductId = g.Key.ProductId,
                //                ProductName = g.Select(e => e.p.Name).FirstOrDefault() ?? "",  // Use product name from join
                //                Sales = new ViewProdGenSales
                //                {
                //                    TotalQtySold = salesDataQuery
                //                            .Where(s => s.ProductId == g.Key.ProductId)
                //                            .Select(s => s.TotalQtySold)
                //                            .FirstOrDefault(),  // Use query instead of materializing

                //                    TotalSales = salesDataQuery
                //                            .Where(s => s.ProductId == g.Key.ProductId)
                //                            .Select(s => s.TotalSales)
                //                            .FirstOrDefault()
                //                },
                //                Generation = new ViewProdGenRecv
                //                {
                //                    TotalQtyGenerated = stockReceivingDataQuery
                //                            .Where(sr => sr.ProductId == g.Key.ProductId)
                //                            .Select(sr => sr.TotalQtyGenerated)
                //                            .FirstOrDefault(),

                //                    AverageGeneration = stockReceivingDataQuery
                //                            .Where(sr => sr.ProductId == g.Key.ProductId)
                //                            .Select(sr => sr.TotalQtyGenerated / DateTime.DaysInMonth(date.Year, date.Month))
                //                            .FirstOrDefault()
                //                },
                //                EstCosting = new ViewProdEstCosting
                //                {
                //                    OverallTotalCost = 0,
                //                    ViewProdEstCostingDetails = estimatedCost.Where(e => e.ProductId == g.Key.ProductId)
                //                                                .Select(e => new ViewProdEstCostingDetails
                //                                                {
                //                                                    CostName = e.CostName ?? "",
                //                                                    TotalCost = e.TotalCost
                //                                                }).ToList() ?? new List<ViewProdEstCostingDetails>()
                //                }
                //            };

                //var result = new ViewGeneratedReportDto
                //{
                //    DateGenerated = date,
                //    TotalExpenses = 0,  // TO DO: Fetch Expenses,
                //    TotalSales = await query.SumAsync(e => e.Sales.TotalSales),
                //    ViewProductGeneratedReportDtos = await query.AsNoTracking().ToListAsync()
                //};
                if (result is null)
                {
                    return ApiResponse<ViewGeneratedReportDto>.Fail("Something went wrong while generating reports...");
                }
                return ApiResponse<ViewGeneratedReportDto>.Success(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<ViewGeneratedReportDto>.Fail("Something went wrong. " + ex.Message.ToString());
            }

            
        }

        /// <summary>
        /// a function that saves the generated report
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<string>> SaveReport(ViewGeneratedReportDto input)
        {
            var isOpen = await CheckIfInventoryStillOpen(input.DateGenerated);
            if (isOpen) return ApiResponse<string>.Fail("Invalid Action! Inventory is still open! Please close before proceeding.");

            var from = new DateTime(input.DateGenerated.Year, input.DateGenerated.Month, 1);
            var to = new DateTime(input.DateGenerated.Year, input.DateGenerated.Month, DateTime.DaysInMonth(input.DateGenerated.Year, input.DateGenerated.Month));

            var reportDetails = new List<ReportDetail>();
            var report = new Report
            {
                Id = Guid.NewGuid(),
                DateGenerated = input.DateGenerated,
                TotalSales = input.TotalSales,
                TotalExpenses = input.TotalExpenses,
                TotalEstimatedCost = input.TotalEstimatedCost,
                From = from,
                To = to
                
            };
            foreach (var item in input.ViewProductGeneratedReportDtos)
            {
                var res = new ReportDetail
                {
                    ReportId = report.Id,
                    ProductId = item.ProductId,
                    AverageGeneration = item.Generation?.AverageGeneration ?? 0m,
                    TotalQtySold = item.Sales?.TotalQtySold ?? 0m,
                    TotalSales = item.Sales?.TotalSales ?? 0m,
                    TotalQtyGenerated = item.Generation?.TotalQtyGenerated ?? 0m,
                    TotalEstimatedCost = item.EstCosting?.OverallTotalCost ?? 0m
                };
                reportDetails.Add(res);
            }

            if (!reportDetails.Any())
            {
                return ApiResponse<string>.Fail("Error! Something went wrong while saving report!");
            }

            await _unitOfWork.Report.AddAsync(report);
            await _unitOfWork.ReportDetail.AddRangeAsync(reportDetails);
            return ApiResponse<string>.Success("Successfully saved report!");
        }

        public async Task<ApiResponse<string>> CloseReport(Guid reportId)
        {
            var report = await _unitOfWork.Report.FirstOrDefaultAsync(e => e.Id == reportId);
            report.IsClosed = true;
            await _unitOfWork.CompleteAsync();
            return ApiResponse<string>.Success("Successfully Closed Report!");
        }

        private async Task<bool> CheckIfInventoryStillOpen(DateTime dateGenerated)
        {
            return await _unitOfWork.InventoryBeginning.GetQueryable()
                .AnyAsync(e => e.CreationTime.Month == dateGenerated.Month && e.Status == InventoryStatus.Open);
        }
    }

}
