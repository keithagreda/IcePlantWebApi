using POSIMSWebApi.Application.Dtos.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.Inventory
{
    public class CurrentInventoryDto
    {
        public string? ProductName { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal SalesQty { get; set; }
        public decimal BegQty { get; set; }
        public decimal ReconciliationQty { get; set; }
        public decimal CurrentStocks { get; set; }
        public int ProductId { get; set; }
    }

    public class GetInventoryDto
    {
        public Guid? InventoryId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal BegQty { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal ReconcilliationQty { get; set; }
        public decimal SalesQty { get; set; }
        public DateTimeOffset? InventoryBegTime { get; set; }
        public DateTimeOffset? InventoryEndTime { get; set;}
    }

    public class GetStockCard
    {
        public ICollection<GetStockCardDayDto> GetStockCardDayDtos { get; set; } = new List<GetStockCardDayDto>();
        public DateTimeOffset? InventoryBegTime { get; set; }
        public DateTimeOffset? InventoryEndTime { get; set; }
    }
    public class GetStockCardDayDto
    {
        public Guid InventoryId { get; set; }
        public DateTime DateTime { get; set; }
        public string Day { get; set; }
        public decimal BegG { get; set; }
        public decimal BegB { get; set; }
        public decimal ReceivingG { get; set; }
        public decimal ReceivingB { get; set; }
        public decimal SalesG { get; set; }
        public decimal SalesB { get; set; }
    }

    public class GetInvDetailsDto
    {
        public decimal GoodQuantity { get; set; }
        public decimal BadQuantity { get; set; }
    }

    public class GetInventoryV1Dto
    {
        public Guid InventoryId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal BegQty { get; set; }
        public DateTimeOffset? InventoryBegTime { get; set; }
        public DateTimeOffset? InventoryEndTime { get; set; }
        public decimal BeginningInvTotal { get; set; }
    }

    public class CurrentInventoryV1Dto
    {
        public int ProductId { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal SalesQty { get; set; }
        public decimal BegQty { get; set; }
        public decimal CurrentStocks { get; set; }
    }

    public class CreateBeginningEntryDto
    {
        public int ProductId { get; set; }
        //public string ProductName { get; set;}
        public decimal ReceivedQty { get; set;}
    }

    public class ProductQuantityDto
    {
        public string? ProductName { get; set; }
        public int ProductId { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class GetAllInventoryDto
    {
        public Guid InventoryBeginningId { get; set; }
        public List<ProductInventoryDto> ProductInventoryDtos { get; set; }
    }

    public class ProductInventoryDto
    {
        public Guid? InventoryId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class InventoryFilter : PaginationParams
    {
        public string? ProductName { get; set; }
        public DateTimeOffset? MinCreationTime { get; set; }
        public DateTimeOffset? MaxCreationTime { get; set; }
        public DateTimeOffset? MinClosedTime { get; set; }
        public DateTimeOffset? MaxClosedTime { get; set; }
    }

    public class InventoryFilterV1 : PaginationParams
    {
        public string? ProductName { get; set; }
        public DateTimeOffset? MinCreationTime { get; set; } = DateTimeOffset.Now.AddDays(-7);
        public DateTimeOffset? MaxCreationTime { get; set; }
        public DateTimeOffset? MinClosedTime { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? MaxClosedTime { get; set; }
    }

    public class ItemData 
    {
        public Guid? InventoryId { get; set; }
        public int ProductId { get; set; }
        public decimal Qty { get; set; }
    }
}
