using Domain.Enums;

namespace POSIMSWebApi.Application.Dtos.Expense
{
    public class CreateExpenseDto
    {
        public ExpenseTypeEnum ExpenseType { get; set; }
        public decimal Amount { get; set; }
    }
}
