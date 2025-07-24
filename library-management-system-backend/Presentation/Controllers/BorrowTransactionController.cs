using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Application.DTOs.BorrowTransaction;
using System.Security.Claims;
using System.Threading.Tasks;
using library_management_system_backend.Application.Interfaces.BorrowTransactions;
using library_management_system_backend.Application.DTOs.BorrowTransaction.library_management_system_backend.Application.DTOs.BorrowTransaction;

namespace library_management_system_backend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BorrowTransactionController : ControllerBase
    {
        private readonly IBorrowTransactionService _borrowTransactionService;

        public BorrowTransactionController(IBorrowTransactionService borrowTransactionService)
        {
            _borrowTransactionService = borrowTransactionService ?? throw new ArgumentNullException(nameof(borrowTransactionService));
        }

        [HttpGet]
        public async Task<IActionResult> GetBorrowTransactions(
            [FromQuery] int? userId = null,
            [FromQuery] string? returnDate = null,
            [FromQuery] bool activeOnly = false)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var authenticatedUserId))
                    return Unauthorized(new { Message = "Invalid user ID in token." });

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                // Restrict userId filter for non-admin/librarian users
                int? filterUserId = (role == "Admin" || role == "Librarian") ? userId : authenticatedUserId;

                var transactions = await _borrowTransactionService.GetBorrowTransactionsAsync(filterUserId, returnDate, activeOnly);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching borrow transactions.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var transaction = await _borrowTransactionService.GetByIdAsync(id);
                if (transaction == null)
                    return NotFound(new { Message = $"Transaction {id} not found." });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { Message = "Invalid user ID in token." });

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role != "Admin" && role != "Librarian" && transaction.UserId != userId)
                    return StatusCode(403, new { Message = "You are not authorized to view this transaction." });

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the transaction.", error = ex.Message });
            }
        }
    }
}