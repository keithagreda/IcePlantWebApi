using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi;

namespace DataAccess.EFCore.Repositories
{
    public class VoidRequestRepository : GenericRepository<VoidRequest>, IVoidRequestRepository
    {
        public VoidRequestRepository(ApplicationContext context) : base(context)
        {

        }
    }
}
