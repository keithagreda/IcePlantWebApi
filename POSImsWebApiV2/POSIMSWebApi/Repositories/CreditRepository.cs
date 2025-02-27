using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi;

namespace DataAccess.EFCore.Repositories
{
    public class CreditRepository : GenericRepository<Credit>, ICreditRepository
    {
        public CreditRepository(ApplicationContext context) : base(context)
        {
        }
    }

    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        public ReportRepository(ApplicationContext context) : base(context)
        {
        }
    }

    public class ReportDetailRepository : GenericRepository<ReportDetail>, IReportDetailRepository
    {
        public ReportDetailRepository(ApplicationContext context) : base(context)
        {
        }
    }

    public class ExpenseRepository : GenericRepository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(ApplicationContext context) : base(context)
        {
        }
    }

}
