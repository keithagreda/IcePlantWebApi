using POSIMSWebApi.Application.Dtos.ProductDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.ProductCost
{
    public class ProductCostDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public int ProductId { get; set; }
    }

    public class CreateOrEditProductCostDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int ProductId { get; set; }
    }

    public class GetProductCostDto : GenericSearchParams
    {
        public bool? IsActive { get; set; }
    }
}
