using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class MachineProduction : AuditedEntity
    {
        public int Id { get; set; }
        public int MachineId { get; set; }

        [ForeignKey("MachineId")]
        public Machine MachineFk { get; set; }

        public Guid? StocksReceivingId { get; set; }

        [ForeignKey("StocksReceivingId")]
        public StocksReceiving StocksReceivingFk { get; set; }

    }
}
