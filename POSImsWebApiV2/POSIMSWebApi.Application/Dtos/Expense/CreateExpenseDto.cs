using Domain.Enums;

namespace POSIMSWebApi.Application.Dtos.Expense
{
    public class CreateOrEditExpenseDto
    {
        public Guid? Id { get; set; }
        public ExpenseTypeEnum ExpenseType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public Guid ReportId { get; set; }
        public DateTime Date { get; set; }
    }
}
