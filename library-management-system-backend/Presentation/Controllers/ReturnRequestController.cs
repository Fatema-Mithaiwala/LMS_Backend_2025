using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using library_management_system_backend.Application.DTOs.ReturnRequestTransaction;
using library_management_system_backend.Application.Interfaces.ReturnRequestTransaction;
using library_management_system_backend.Domain.Enums;

namespace library_management_system_backend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReturnRequestController : ControllerBase
    {
        private readonly IReturnRequestService _returnRequestService;

        public ReturnRequestController(IReturnRequestService returnRequestService)
        {
            _returnRequestService = returnRequestService ?? throw new ArgumentNullException(nameof(returnRequestService));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> GetAll([FromQuery] string? status = null)
        {
            if (!string.IsNullOrEmpty(status))
            {
                var validStatuses = Enum.GetNames(typeof(BorrowRequestStatus)).Select(s => s.ToLower()).ToList();
                if (!validStatuses.Contains(status.ToLower()))
                {
                    return BadRequest(new { Message = $"Invalid status value: {status}. Valid values are: {string.Join(", ", validStatuses)}" });
                }
            }

            var requests = await _returnRequestService.GetAllAsync(status);
            return Ok(requests);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { Message = "Invalid user ID in token." });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            int? filterUserId = (role == "Admin" || role == "Librarian") ? null : userId;

            var requests = await _returnRequestService.GetPendingAsync(filterUserId);
            return Ok(requests);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create([FromBody] CreateReturnRequestDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { Message = "Invalid user ID in token." });

            await _returnRequestService.CreateAsync(userId, dto);
            return Ok(new { Message = "Return request created successfully." });
        }

        [HttpPost("approve/{requestId}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Approve(int requestId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var processorId))
                return Unauthorized(new { Message = "Invalid processor ID in token." });

            await _returnRequestService.ApproveAsync(requestId, processorId);
            return Ok(new { Message = "Return request approved successfully." });
        }

        [HttpPost("reject/{requestId}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Reject(int requestId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var processorId))
                return Unauthorized(new { Message = "Invalid processor ID in token." });

            await _returnRequestService.RejectAsync(requestId, processorId);
            return Ok(new { Message = "Return request rejected successfully." });
        }
    }
}