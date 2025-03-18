using Domain.Interfaces;
using POSIMSWebApi;
using POSIMSWebApi.Authentication;

namespace DataAccess.EFCore.Repositories
{
    public class ApplicationIdentityUserRepository : GenericRepository<ApplicationIdentityUser>, IApplicationIdentityUser
    {
        public ApplicationIdentityUserRepository(ApplicationContext context) : base(context)
        {

        }
    }
}
