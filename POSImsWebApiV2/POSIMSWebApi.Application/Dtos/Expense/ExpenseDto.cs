using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.Expense
{
    public class ExpenseDto
    {
        public Guid Id { get; set; }
        public ExpenseTypeEnum ExpenseType { get; set; }
        public decimal Amount { get; set; }

        // Navigation Property
        public Guid ReportId { get; set; }
    }
}
