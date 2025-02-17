using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi;

namespace DataAccess.EFCore.Repositories
{
    public class StockReconciliationRepository : GenericRepository<StockReconciliation>, IStockReconciliationRepository
    {
        public StockReconciliationRepository(ApplicationContext context) : base(context)
        {
        }
    }

}
