﻿using POSIMSWebApi.Application.Dtos.ProductDtos;
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

    public class GetProductCostingDto
    {
        public string ProductName { get; set; }
        public decimal OverallEstimatedCost { get; set; }
        public ICollection<ProductCosting> ProductCosting { get; set; } = new List<ProductCosting>();

    }

    public class ProductCosting
    {
        public string CostName { get; set; }
        public decimal TotalEstimatedCost { get; set; }
    }

    public class CreateOrEditProductCostDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int ProductId { get; set; }
    }

    public class GetProductCostInput : GenericSearchParams
    {
        public bool? IsActive { get; set; }
    }
}
