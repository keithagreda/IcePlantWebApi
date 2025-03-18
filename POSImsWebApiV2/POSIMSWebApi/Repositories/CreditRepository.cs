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

    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationContext context) : base(context)
        {
        }
    }


}
