using Domain.Entities;
using POSIMSWebApi.Application.Dtos.ProductDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.InventoryReconcillation
{
    public class InventoryReconcillationDto
    {
        public int Id { get; set; }
        /// <summary>
        /// can be negative
        /// </summary>
        public int Quantity { get; set; }
        public Guid? RemarksId { get; set; }
        public Guid InventoryBeginningId { get; set; }
        public int ProductId { get; set; }
    }

    public class CreateInventoryReconcillationDto
    {
        public decimal Quantity { get; set; }
        public int ProductId { get; set; }
        //public Guid RemarksId { get; set; }
        //public Guid InventoryBeginningId { get; set; }
    }

    public class GetInventoryReconcillationInput : GenericSearchParams
    {
    }

    public class GetInventoryReconcillation
    {
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Remarks { get; set; }
        public bool IsInventoryOpen { get; set; }
    }

}
