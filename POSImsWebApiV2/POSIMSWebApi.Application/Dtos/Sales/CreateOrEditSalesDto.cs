﻿using Domain.Enums;
using POSIMSWebApi.Application.Dtos.Pagination;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.Sales
{
    public class CreateOrEditSalesDto
    {
        public Guid? SalesHeaderId { get; set; }
        public Guid? CustomerId { get; set; }
        [Required]
        public List<CreateSalesDetailDto> CreateSalesDetailDtos { get; set; } = new List<CreateSalesDetailDto>();
    }

    public class FilterSales : PaginationParams
    {
        public string? TransNum { get; set; }
        public DateTime? MinTransDate { get; set; }
        public DateTime? MaxTransDate { get; set; }
    }

    public class SalesHeaderDto
    {
        public Guid Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string? TransNum { get; set; }
        public DateTime TransactionDate { get; set; }
        public string SoldBy { get; set; }
        public string CustomerName { get; set; }
        public List<SalesDetailDto> SalesDetailsDto { get; set; } = new List<SalesDetailDto>();
        public bool? IsInventoryClosed { get; set; }
    }

    public class SalesDetailDto
    {
        public decimal ProductPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal Discount { get; set; }
        public string ProductName { get; set; }
        public decimal ActualSellingPrice { get; set; }
        public decimal Amount { get; set; }
    }

    public class SalesSummaryDto
    {
        public string CustomerName { get; set; }
        public string? SoldBy { get; set; }
        public string? TransNum { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalPrice { get; set; }
        public InventoryStatus CurrentInventory { get; set; }
    }

    public class SalesSummaryWithEst
    {
        //purpose of this is to total the sales based on the filter
        public decimal TotalSales { get; set; }
        //sales based on inventory opening and closing
        public decimal DailySales { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public ICollection<SalesSummaryDto> SalesSummaryDtos { get; set; } = new List<SalesSummaryDto>();
    }

}
