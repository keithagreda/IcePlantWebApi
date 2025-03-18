namespace Domain.Entities
{
    public class Notification : AuditedEntity 
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsRead { get; set; }
        public string SentTo { get; set; }
    }
}
