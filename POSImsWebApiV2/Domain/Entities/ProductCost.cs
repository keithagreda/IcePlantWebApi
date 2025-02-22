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

        [ForeignKey("ProductCostId")]
        public ProductCost ProductCost { get; set; }

        public Guid? SalesHeaderId { get; set; }

        [ForeignKey("SalesHeaderId")]
        public SalesHeader SalesHeaderFk { get; set; }

    }
}
