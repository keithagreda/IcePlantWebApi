using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi;

namespace DataAccess.EFCore.Repositories
{
    public class ProductCostRepository : GenericRepository<ProductCost>, IProductCostRepository
    {
        public ProductCostRepository(ApplicationContext context) : base(context)
        {
        }
    }

}
