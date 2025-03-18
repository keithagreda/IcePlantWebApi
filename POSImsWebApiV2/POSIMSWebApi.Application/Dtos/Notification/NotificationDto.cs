using POSIMSWebApi.Application.Dtos.ProductDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.Notification
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsRead { get; set; }
        public string SentTo { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class GetNotificationDto : NotificationDto
    {
        public string SentToName { get; set; }
        public string StrCreationTime { get; set; }
    }

    public class GetNotificationInputDto : GenericSearchParams
    {
        public DateTime? MinDateTime { get; set; }
        public DateTime? MaxDateTime { get; set; }
    }
    
    public class CreateNotificationDto
    {
        public string Title { get; set; }
        public string Desc { get; set; }
    }
}
