using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class StocksDetail : AuditedEntity
    {
        public int Id { get; set; }
        public string StockNum { get; set; }
        public int StockNumInt { get; set; }
        public int StocksHeaderId { get; set; }
        [ForeignKey("StocksHeaderId")]
        public StocksHeader StocksHeaderFk { get; set; }
        public bool Unavailable { get; set; }
    }

    public class VoidRequest : AuditedEntity 
    {
        public Guid Id { get; set; }
        public VoidRequestStatus Status { get; set; }
        public Guid? SalesHeaderId { get; set; }

        [ForeignKey("SalesHeaderId")]
        public SalesHeader SalesHeaderFk { get; set; }
        public string? ApproverId { get; set; }
    }

}
