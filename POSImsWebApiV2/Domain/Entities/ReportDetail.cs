using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ReportDetail : AuditedEntity
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public decimal TotalQtyGenerated { get; set; }
        public decimal TotalQtySold { get; set; }
        public decimal TotalSales { get; set; }
        public decimal AverageGeneration { get; set; }
        public decimal TotalEstimatedCost { get; set; }

        // Navigation Property
        public Guid ReportId { get; set; }
        [ForeignKey("ReportId")]
        public Report Report { get; set; }
    }


}
