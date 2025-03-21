using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{

    public class InventoryReconciliation : AuditedEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// can be negative
        /// </summary>
        public decimal Quantity { get; set; }
        public Guid? RemarksId { get; set; }

        [ForeignKey("RemarksId")]
        public Remarks RemarksFk { get; set; }

        public Guid InventoryBeginningId { get; set; }
        [ForeignKey("InventoryBeginningId")]
        public InventoryBeginning InventoryBeginningFk { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product ProductFk { get; set; }

    }
}
