using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using library_management_system_backend.Application.DTOs.Notifications;
using library_management_system_backend.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using library_management_system_backend.Application.Interfaces.Notifications;

namespace library_management_system_backend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserIdFromClaims();
            var notifications = await _notificationService.GetByUserIdAsync(userId, unreadOnly, page, pageSize);
            return Ok(notifications);
        }

        [HttpPatch("{id}/read")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserIdFromClaims();
            await _notificationService.MarkAsReadAsync(id, userId);
            return Ok(new { Message = "Notification marked as read." });
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            return userId;
        }
    }
}