namespace POSIMSWebApi.Application.Dtos.ReportDetail
{
    public class CreateReportDetailDto
    {
        public int ProductId { get; set; }
        public int TotalQtyGenerated { get; set; }
        public int TotalQtySold { get; set; }
        public decimal TotalSales { get; set; }
        public decimal AverageGeneration { get; set; }

        // Navigation Property
        public Guid ReportId { get; set; }
    }
}
