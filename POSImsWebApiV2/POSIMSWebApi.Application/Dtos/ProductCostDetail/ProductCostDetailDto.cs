namespace POSIMSWebApi.Application.Dtos.ProductCostDetail
{
    public class ProductCostDetailDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public int? ProductCostId { get; set; }
        public Guid? SalesHeaderId { get; set; }
    }
}
