using Domain.Entities;
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
}
