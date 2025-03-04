namespace Domain.Entities
{
    public class Report : AuditedEntity
    {
        public Guid Id { get; set; }
        public DateTime DateGenerated { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public decimal TotalExpenses { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public bool IsClosed { get; set; }

        // Navigation Properties
        public ICollection<ReportDetail> ReportDetails { get; set; } = new List<ReportDetail>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }

    
}
