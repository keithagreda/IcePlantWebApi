using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{

    public class  MachineProduction : AuditedEntity
    {
        public int Id { get; set; }
        public int MachineId { get; set; }

        [ForeignKey("MachineId")]
        public Machine MachineFk { get; set; }

        public Guid InventoryBeginningId { get; set; }
        [ForeignKey("InventoryBeginningId")]
        public InventoryBeginning InventoryBeginningFk { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product ProductFk { get; set; }
        public decimal Quantity { get; set; }
        public string TransNum { get; set; }
        //possibility to add price here for analytics
    }
}
