using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using library_management_system_backend.Application.DTOs.Wishlists;
using library_management_system_backend.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using library_management_system_backend.Application.Interfaces.Whishlists.library_management_system_backend.Application.Interfaces;

namespace library_management_system_backend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService ?? throw new ArgumentNullException(nameof(wishlistService));
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = GetUserIdFromClaims();
            var wishlists = await _wishlistService.GetByUserIdAsync(userId);
            return Ok(wishlists);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> GetAllWishlists()
        {
            var wishlists = await _wishlistService.GetAllAsync();
            return Ok(wishlists);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AddWishlistItem([FromBody] CreateWishlistDto dto)
        {
            var userId = GetUserIdFromClaims();
            await _wishlistService.AddWishlistItemAsync(userId, dto);
            return Ok(new { Message = "Book added to wishlist successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RemoveWishlistItem(int id)
        {
            var userId = GetUserIdFromClaims();
            await _wishlistService.RemoveWishlistItemAsync(id, userId);
            return Ok(new { Message = "Book removed from wishlist successfully." });
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