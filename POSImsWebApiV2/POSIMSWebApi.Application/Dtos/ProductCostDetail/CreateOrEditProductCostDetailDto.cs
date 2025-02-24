using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.ProductCostDetail
{
    public class CreateOrEditProductCostDetailDto
    {
        public Guid? Id { get; set; }
        public decimal Amount { get; set; }
        public int ProductCostId { get; set; }
        public Guid StocksReceivingId { get; set; }
    }
}
