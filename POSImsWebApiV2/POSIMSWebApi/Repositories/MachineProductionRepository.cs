using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi;

namespace DataAccess.EFCore.Repositories
{
    public class MachineProductionRepository : GenericRepository<MachineProduction>, IMachineProductionRepository
    {
        public MachineProductionRepository(ApplicationContext context) : base(context)
        {
        }
    }

}
