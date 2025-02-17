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
    }
}
