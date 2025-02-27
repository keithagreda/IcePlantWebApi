using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.ReportDetail
{
    public class ReportDetailDto
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public int TotalQtyGenerated { get; set; }
        public int TotalQtySold { get; set; }
        public decimal TotalSales { get; set; }
        public decimal AverageGeneration { get; set; }

        // Navigation Property
        public Guid ReportId { get; set; }
    }
}
