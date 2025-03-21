using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi;

namespace DataAccess.EFCore.Repositories
{
    public class InventoryReconciliationRepository : GenericRepository<InventoryReconciliation>, IInventoryReconciliationRepository
    {
        public InventoryReconciliationRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
