using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class StockReconciliation : AuditedEntity
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string TransNum { get; set; }
        public Guid? RemarksId { get; set; }

        [ForeignKey("RemarksId")]
        public Remarks RemarksFk { get; set; }

    }
}
