namespace Domain.Entities
{
    public class Machine : AuditedEntity
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
