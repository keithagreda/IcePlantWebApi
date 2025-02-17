using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi;

namespace DataAccess.EFCore.Repositories
{
    public class MachineRepository : GenericRepository<Machine>, IMachineRepository
    {
        public MachineRepository(ApplicationContext context) : base(context)
        {
        }
    }

}
