using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using library_management_system_backend.Application.DTOs.BorrowRequestTransaction;
using library_management_system_backend.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using library_management_system_backend.Domain.Enums;

namespace library_management_system_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BorrowRequestController : ControllerBase
    {
        private readonly IBorrowRequestService _borrowRequestService;

        public BorrowRequestController(IBorrowRequestService borrowRequestService)
        {
            _borrowRequestService = borrowRequestService ?? throw new ArgumentNullException(nameof(borrowRequestService));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<ActionResult<IEnumerable<BorrowRequestDto>>> GetAll([FromQuery] string? status = null)
        {
            if (!string.IsNullOrEmpty(status))
            {
                var validStatuses = Enum.GetNames(typeof(BorrowRequestStatus)).Select(s => s.ToLower()).ToList();
                if (!validStatuses.Contains(status.ToLower()))
                {
                    return BadRequest(new { Message = $"Invalid status value: {status}. Valid values are: {string.Join(", ", validStatuses)}" });
                }
            }

            var requests = await _borrowRequestService.GetAllAsync(status);
            return Ok(requests);
        }


        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<BorrowRequestDto>>> GetPending()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { Message = "Invalid user ID in token." });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            int? filterUserId = (role == "Admin" || role == "Librarian") ? null : userId;

            var requests = await _borrowRequestService.GetPendingAsync(filterUserId);
            return Ok(requests);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> Create([FromBody] CreateBorrowRequestDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { Message = "Invalid user ID in token." });

            await _borrowRequestService.CreateAsync(userId, dto);
            return Ok(new { Message = "Borrow request created successfully" });
        }

        [HttpPost("approve/{requestId}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<ActionResult> Approve(int requestId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var approverId))
                return Unauthorized(new { Message = "Invalid approver ID in token." });

            await _borrowRequestService.ApproveAsync(requestId, approverId);
            return Ok(new { Message = "Borrow request approved successfully" });
        }

        [HttpPost("reject/{requestId}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<ActionResult> Reject(int requestId, [FromBody] string? remarks)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var approverId))
                return Unauthorized(new { Message = "Invalid approver ID in token." });

            await _borrowRequestService.RejectAsync(requestId, approverId, remarks);
            return Ok(new { Message = "Borrow request rejected successfully" });
        }
    }
}