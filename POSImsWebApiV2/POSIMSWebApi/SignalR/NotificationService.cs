using Domain.ApiResponse;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using POSIMSWebApi.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Services
{
    //[Authorize]
    //public class NotificationService
    //{
    //    private readonly IUnitOfWork _unitOfWork;
    //    private readonly IHubContext<NotificationHub> _hubContext;

    //    public NotificationService(IUnitOfWork unitOfWork, IHubContext<NotificationHub> hubContext)
    //    {
    //        _unitOfWork = unitOfWork;
    //        _hubContext = hubContext;
    //    }
    //    public async Task SendNotification(string message)
    //    {
    //        await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
    //    }
    //}
}
