using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.StocksReconciliation
{
    public class StocksReconciliationDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string TransNum { get; set; }
        public Guid? RemarksId { get; set; }
    }

    public class CreateOrEditStocksReconciliationDto
    {
        public int? Id { get; set; }
        public int Quantity { get; set; }
        public string TransNum { get; set; }
        public string? Remarks { get; set; }
    }

    public class GetReconcileProductDto
    {
        public TransactionTypeEnum TransType { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string TransNum { get; set; }
    }
}
