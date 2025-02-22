using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi;

namespace DataAccess.EFCore.Repositories
{
    public class ProductCostDetailsRepository : GenericRepository<ProductCostDetails>, IProductCostDetailsRepository
    {
        public ProductCostDetailsRepository(ApplicationContext context) : base(context)
        {
        }
    }

}
