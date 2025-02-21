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

    public class CreateOrEditMachineProductionDto
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public Guid? StocksReceivingId { get; set; }
    }

    public class GetMachineGenerationDto
    {
        public string MachineName { get; set; }
        public List<MachineGenerationQty> MachineGenerationQtys { get; set; } = new List<MachineGenerationQty>();
    }

    public class GetMachineGenerationV1Dto
    {
        public string MachineName { get; set; }
        public decimal Good { get; set; }
        public decimal Bad { get; set; }
    }

    public class GetMachineGenerationWTotal
    {
        public decimal TotalGood { get; set; }
        public decimal TotalGoodPercentage { get; set; }
        public List<GetMachineGenerationV1Dto> GetMachineGenerationV1Dtos { get; set; } = new List<GetMachineGenerationV1Dto>();
    }

    public class MachineGenerationQty
    {
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
    }

    public class GetMachineGenerationInput : GenericSearchParams
    {
        public DateTime? MinCreationTime { get; set; }
        public DateTime? MaxCreationTime { get; set; }
    }
}
