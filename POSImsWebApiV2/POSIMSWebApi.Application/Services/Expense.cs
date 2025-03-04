using Domain.ApiResponse;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Expense;
using POSIMSWebApi.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ExpenseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// a function that creates or edits an expense
        /// Returns 1 if success
        /// returns 0 if fail
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ApiResponse<string>> CreateOrEditExpense(CreateOrEditExpenseDto input)
        {

            var isClosed = await CheckIfClosed(input.Date);
            if (!isClosed.IsSuccess)
            {
                return isClosed;
            }

            if (input.Id is null)
            {
                return await CreateExpense(input);
            }
            return await EditExpense(input);
        }

        private async Task<ApiResponse<string>> CreateExpense(CreateOrEditExpenseDto input)
        {
            var mapToExpenseEntity = new Expense
            {
                Amount = input.Amount,
                ExpenseType = input.ExpenseType,
                Description = input.Description,
                ReportId = input.ReportId
            };

            await _unitOfWork.Expense.AddAsync(mapToExpenseEntity);
            return ApiResponse<string>.Success("Expense succesfully added!");
        }

        private async Task<ApiResponse<string>> EditExpense(CreateOrEditExpenseDto input)
        {
            var existing = await _unitOfWork.Expense.FirstOrDefaultAsync(e => e.Id == input.Id);
            if (existing is null)
            {
                return ApiResponse<string>.Fail("Error! Expense To Edit Not Found!");
            }
            existing.ExpenseType = input.ExpenseType;
            existing.Description = input.Description;
            existing.ReportId = input.ReportId;
            existing.Amount = input.Amount;
            return ApiResponse<string>.Success("Expense has been added!");
        }

        //make validation that checks if a report has already been made for the month
        private async Task<ApiResponse<string>> CheckIfClosed(DateTime input)
        {
            var isClosed = await _unitOfWork.Report.GetQueryable().AnyAsync(e => e.CreationTime.Month == input.Date.Month && e.IsClosed == true);
            if (isClosed) return ApiResponse<string>.Fail("Invalid Action! Report has already been closed!");
            return ApiResponse<string>.Success("Report is still not closed!");
        }
    }
}
