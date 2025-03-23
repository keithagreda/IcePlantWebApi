using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi;

namespace DataAccess.EFCore.Repositories
{
    public class PrinterLogsRepository : GenericRepository<PrinterLogs>, IPrinterLogsRepository
    {
        public PrinterLogsRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
