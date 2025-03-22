using Domain.ApiResponse;
using Domain.Entities;
using Domain.Interfaces;
using Humanizer;
using LanguageExt;
using LanguageExt.Pipes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Notification;
using POSIMSWebApi.Application.Dtos.Pagination;
using POSIMSWebApi.Authentication;
using POSIMSWebApi.QueryExtensions;
using POSIMSWebApi.SignalR;
using System.Security.Claims;

namespace POSIMSWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        public NotificationController(IUnitOfWork unitOfWork, UserManager<ApplicationIdentityUser> userManager, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _hubContext = hubContext;
        }
        [HttpPost("CreateNotification")]
        public async Task<ActionResult<ApiResponse<string>>> CreateNotification(CreateNotificationDto input)
        {
            List<Notification> notifications = new List<Notification>();

            var admins = (await _userManager.GetUsersInRoleAsync("Admin")).Select(e => e.Id).ToList();

            await SendMessageToAdmin("You have a new notification!");

            foreach (var admin in admins)
            {
                Notification notification = new Notification
                {
                    Title = input.Title,
                    Description = input.Desc,
                    SentTo = admin,
                    RedirectTo = input.RedirectTo
                };
                notifications.Add(notification);
            }
            if(notifications is null)
            {
                return Ok(ApiResponse<string>.Fail("Invalid Action! Must Create Admin User First!"));
            }
            await _unitOfWork.Notification.AddRangeAsync(notifications);
            await _unitOfWork.CompleteAsync();
            return Ok(ApiResponse<string>.Success("Successfully sent notification to admins"));
        }
        [HttpPost("SetNotificationToRead")]
        public async Task<ActionResult<ApiResponse<string>>> SetNotificationToRead(Guid id)
        {
            var notification = await _unitOfWork.Notification.FirstOrDefaultAsync(e => e.Id == id);
            if(notification is null)
            {
                return BadRequest("Error! Notification Not Found");
            }

            if (notification.IsRead)
            {
                return Ok(ApiResponse<string>.Fail("Invalid Action! Notification has already been read"));
            }

            notification.IsRead = true;
            await _unitOfWork.CompleteAsync();
            return Ok(ApiResponse<string>.Success("Success!"));
        }
        [HttpGet("GetAllNotification")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<GetNotificationDto>>>> GetAllNotification([FromQuery]GetNotificationInputDto input)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //DateTime minDateTime = input.MinDateTime != null ? (DateTime)input.MinDateTime : DateTime.UtcNow.AddDays(-7);
                //DateTime maxDateTime = input.MaxDateTime != null ? (DateTime)input.MaxDateTime : DateTime.UtcNow;
                var context = _unitOfWork.Notification.GetQueryable()
                    .Where(e => e.SentTo == userId);

                var count = await context.CountAsync();

                if (count <= 0)
                {
                    return Ok(ApiResponse<PaginatedResult<GetNotificationDto>>.Success(new PaginatedResult<GetNotificationDto>([], 0, (int)input.PageNumber, (int)input.PageSize)));
                }

                var notifications = await context
                    .OrderByDescending(e => e.CreationTime)
                    //.WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => false || e.Title == input.FilterText || e.Description == input.FilterText)
                    .Select(e => new GetNotificationDto
                    {
                        SentToName = "",
                        StrCreationTime = "",
                        Description = e.Description,
                        Id = e.Id,
                        Title = e.Title,
                        IsRead = e.IsRead,
                        CreationTime = e.CreationTime,

                    })
                    .ToPaginatedResult((int)input.PageNumber, (int)input.PageSize)
                    .ToListAsync();

                //humanize
                foreach (var item in notifications)
                {
                    item.StrCreationTime = item.CreationTime.Humanize();
                }

                return Ok(ApiResponse<PaginatedResult<GetNotificationDto>>
                    .Success(new PaginatedResult<GetNotificationDto>(notifications, count, (int)input.PageNumber, (int)input.PageSize)));
            }
            catch (Exception ex)
            {

                return BadRequest(ex);
            }
        }
        [HttpPost("SendMessageToAdmin")]
        public async Task<ActionResult<ApiResponse<string>>> SendMessageToAdmin(string message)
        {
            await _hubContext.Clients.All.SendAsync("AdminNotification", message);
            return Ok(ApiResponse<string>.Success(""));
        }
        [HttpPost("TestMessage")]
        public async Task<ActionResult<ApiResponse<string>>> TestMessage(string message)
        {
            await _hubContext.Clients.All.SendAsync("AdminNotification", message);
            return Ok(ApiResponse<string>.Success(""));
        }
    }
}
