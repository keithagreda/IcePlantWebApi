using Domain.ApiResponse;
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
                //check if existing 
                //TO DO!! MAKE THIS DYNAMIC
                var existing = await _unitOfWork.Report.GetQueryable().AnyAsync(e => e.DateGenerated.Month == date.Month);
                if (existing)
                {
                    //make a response that the report already exists
                    return ApiResponse<ViewGeneratedReportDto>.Fail("Something went wrong while generating reports...");
                }
                //generate month now for 
                //generate report
                //MUST INCLUDE VERY FIRST BEG QTY

                // 1️⃣ Fetch Sales Data Once
                // 1️⃣ Fetch Sales Data as IQueryable (Deferred Execution)
                var salesDataQuery = _unitOfWork.SalesDetail.GetQueryable()
                    .Include(sd => sd.SalesHeaderFk)
                    .ThenInclude(sh => sh.InventoryBeginningFk)
                    .Where(e => e.SalesHeaderFk.InventoryBeginningFk.CreationTime.Month == date.Month)
                    .GroupBy(sd => sd.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQtySold = g.Sum(sd => sd.Quantity),
                        TotalSales = g.Sum(sd => sd.Amount)
                    });

                // 2️⃣ Fetch Stock Receiving Data as IQueryable
                var stockReceivingDataQuery = _unitOfWork.StocksReceiving.GetQueryable()
                    .Include(sr => sr.StocksHeaderFk)
                    .Include(e => e.InventoryBeginningFk)
                    .Where(e => e.InventoryBeginningFk.CreationTime.Month == date.Month)
                    .GroupBy(sr => sr.StocksHeaderFk.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQtyGenerated = g.Sum(sr => sr.Quantity)
                    });

                // 3️⃣ Main Query: Keep Sales & Stock Queries as Subqueries
                var query = from ibd in _unitOfWork.InventoryBeginningDetails.GetQueryable()
                            join ib in _unitOfWork.InventoryBeginning.GetQueryable()
                                on ibd.InventoryBeginningId equals ib.Id
                            join p in _unitOfWork.Product.GetQueryable()
                                on ibd.ProductId equals p.Id
                            where ib.CreationTime.Month == date.Month
                            group new { ibd, p } by new { ibd.ProductId } into g
                            select new ViewProductGeneratedReportDto
                            {
                                ProductId = g.Key.ProductId,
                                    ProductName = g.Select(e => e.p.Name).FirstOrDefault() ?? "",  // Use product name from join
                                    Sales = new ViewProdGenSales
                                    {
                                        TotalQtySold = salesDataQuery
                                            .Where(s =>  s.ProductId == g.Key.ProductId)
                                            .Select(s => s.TotalQtySold)
                                            .FirstOrDefault(),  // Use query instead of materializing

                                        TotalSales = salesDataQuery
                                            .Where(s => s.ProductId == g.Key.ProductId)
                                            .Select(s => s.TotalSales)
                                            .FirstOrDefault()
                                    },
                                    Generation = new ViewProdGenRecv
                                    {
                                        TotalQtyGenerated = stockReceivingDataQuery
                                            .Where(sr => sr.ProductId == g.Key.ProductId)
                                            .Select(sr => sr.TotalQtyGenerated)
                                            .FirstOrDefault(),

                                        AverageGeneration = stockReceivingDataQuery
                                            .Where(sr =>  sr.ProductId == g.Key.ProductId)
                                            .Select(sr => sr.TotalQtyGenerated / DateTime.DaysInMonth(date.Year, date.Month))
                                            .FirstOrDefault()
                                    }
                            };

                var result = new ViewGeneratedReportDto
                {
                    DateGenerated = date,
                    TotalExpenses = 0,  // TO DO: Fetch Expenses,
                    TotalSales = await query.SumAsync(e => e.Sales.TotalSales),
                    ViewProductGeneratedReportDtos = await query.AsNoTracking().ToListAsync()
                };
                if (result is null)
                {
                    return ApiResponse<ViewGeneratedReportDto>.Fail("Something went wrong while generating reports...");
                }
                return ApiResponse<ViewGeneratedReportDto>.Success(result);
            }
            catch (Exception ex )
            {

                return ApiResponse<ViewGeneratedReportDto>.Fail("Something went wrong."  + ex.Message.ToString());
            }
            ////get sales for the month for each product
            //var sales = _unitOfWork.SalesHeader.GetQueryable()
            //    .Include(e => e.SalesDetails)
            //    .ThenInclude(e => e.ProductFk)
            //    .Where(e => e.CreationTime.Month == date.Month)
            //    .GroupBy(e => e.SalesDetails.Select(e => e.ProductFk.Name))
            //    .Select(e => new
            //    {
            //        ProductName = e.Key,
            //        Quantity = e.Sum(e => e.SalesDetails.Sum(e => e.Quantity)),
            //        Total = e.Sum(e => e.SalesDetails.Sum(e => e.Amount))
            //    });

            ////get product generation for the month


            //var receive = _unitOfWork.StocksReceiving.GetQueryable()
            //    .Include(e => e.StocksHeaderFk)
            //    .ThenInclude(e => e.ProductFK)
            //    .Where(e => e.CreationTime.Month == date.Month)
            //    .GroupBy(e => e.StocksHeaderFk.ProductFK.Name)
            //    .Select(e => new
            //    {
            //        ProductName = e.Key,
            //        Quantity = e.Sum(e => e.Quantity),
            //        AverageDailyGeneration = e.Where(e => e.CreationTime.Month == date.Month).Select(e => e.Quantity).Sum() / DateTime.DaysInMonth(date.Year, date.Month)
            //    });

            ////get product cost for estimation
            //var productCost = _unitOfWork.ProductCost.GetQueryable()
            //    .Include(e => e.ProductFk)
            //    .Include(e => e.ProductCostDetails)
            //    .Where(e => e.CreationTime.Month == date.Month)
            //    .GroupBy(e => new { ProductName = e.ProductFk.Name , e.Name})
            //    .Select(e => new
            //    {
            //        CostName = e.Key.Name,
            //        ProductName = e.Key.ProductName,
            //        Cost = e.Sum(e => e.ProductCostDetails.Sum(e => e.ProductCostTotalAmount))
            //    });

            //return report
        }
        /// <summary>
        /// a function that saves the generated report
        /// </summary>
        /// <returns></returns>
        //public async Task<ApiResponse<int>> SaveReport()
        //{

        //} 
    }
}
