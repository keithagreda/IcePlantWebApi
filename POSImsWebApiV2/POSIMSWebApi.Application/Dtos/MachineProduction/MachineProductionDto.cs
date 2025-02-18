using Domain.Entities;
using POSIMSWebApi.Application.Dtos.ProductDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.MachineProduction
{
    public  class MachineProductionDto
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public Guid? StocksReceivingId { get; set; }
    }

    public class GetMachineGenerationDto
    {
        public int Id { get; set; }
        public string MachineName { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
    }

    public class GetMachineGenerationInput : GenericSearchParams
    {
        public DateTime? MinCreationTime { get; set; }
        public DateTime? MaxCreationTime { get; set; }
    }
}
