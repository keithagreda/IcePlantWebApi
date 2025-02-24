using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ProductCost : AuditedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public decimal Amount { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product ProductFk { get; set; }
        public ICollection<ProductCostDetails> ProductCostDetails { get; set; } = new List<ProductCostDetails>();

    }

    public class ProductCostDetails : AuditedEntity
    {
        public Guid Id { get; set; }
        public int? ProductCostId { get; set; }
        //dapat naka multiply na daan ang qty og costing 
        public decimal ProductCostTotalAmount { get; set; }

        [ForeignKey("ProductCostId")]
        public ProductCost ProductCost { get; set; }

        public Guid? StocksReceivingId { get; set; }

        [ForeignKey("StocksReceivingId")]
        public StocksReceiving StocksReceivingFk { get; set; }


    }
}
