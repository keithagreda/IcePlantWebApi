using Domain.ApiResponse;
using POSIMSWebApi.Application.Dtos.Expense;

namespace POSIMSWebApi.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<ApiResponse<string>> CreateOrEditExpense(CreateOrEditExpenseDto input);
    }
}