namespace Domain.Entities
{
    public class PrinterLogs : AuditedEntity
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
