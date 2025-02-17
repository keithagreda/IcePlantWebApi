using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Credit : AuditedEntity
    {
        public int Id { get; set; }
        public string CreditName { get; set; }
        public int Amount { get; set; }
        public Guid? InventoryBeginningId { get; set; }

        [ForeignKey("InventoryBeginningId")]
        public InventoryBeginning InventoryBeginningFk { get; set; }
    }
}
