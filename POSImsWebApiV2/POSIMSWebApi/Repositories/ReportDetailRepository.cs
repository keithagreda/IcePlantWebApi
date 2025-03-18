using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi;

namespace DataAccess.EFCore.Repositories
{
    public class ReportDetailRepository : GenericRepository<ReportDetail>, IReportDetailRepository
    {
        public ReportDetailRepository(ApplicationContext context) : base(context)
        {
        }
    }

}
