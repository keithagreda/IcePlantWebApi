using Domain.Entities;
using POSIMSWebApi.Application.Dtos.Expense;
using POSIMSWebApi.Application.Dtos.ReportDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.Report
{
    public class ReportDto
    {
        public Guid Id { get; set; }
        public DateTime DateGenerated { get; set; }

        // Navigation Properties
        public ICollection<ReportDetailDto> ReportDetails { get; set; } = new List<ReportDetailDto>();
        public ICollection<ExpenseDto> Expenses { get; set; } = new List<ExpenseDto>();
    }

    public class ViewGeneratedReportDto
    {
        public DateTime DateGenerated { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public ICollection<ViewProductGeneratedReportDto> ViewProductGeneratedReportDtos { get; set; }
    }

    public class ViewProductGeneratedReportDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public ViewProdGenSales? Sales { get; set; }
        public ViewProdGenRecv? Generation { get; set; }
        public ViewProdEstCosting? EstCosting { get; set; }

    }

    public class ViewProdGenSales
    {
        public decimal TotalQtySold { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class ViewProdGenRecv
    {
        public decimal TotalQtyGenerated { get; set; }

        public decimal AverageGeneration { get; set; }
    }

    public class ViewProdEstCosting
    {
        public decimal OverallTotalCost { get; set; }
        public ICollection<ViewProdEstCostingDetails> ViewProdEstCostingDetails { get; set; } = new List<ViewProdEstCostingDetails>();
    }

    public class ViewProdEstCostingDetails
    {
        public string CostName { get; set; }
        public decimal TotalCost { get; set; }
    }
}
