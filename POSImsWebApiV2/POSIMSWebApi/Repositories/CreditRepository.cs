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

}
