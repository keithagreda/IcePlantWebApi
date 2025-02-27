using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Expense : AuditedEntity
    {
        public Guid Id { get; set; }
        public ExpenseTypeEnum ExpenseType { get; set; }
        public decimal Amount { get; set; }

        // Navigation Property
        public Guid ReportId { get; set; }
        [ForeignKey("ReportId")]
        public Report Report { get; set; }
    }
}
