using library_management_system_backend.Application.DTOs;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace library_management_system_backend.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred", detail = ex.Message });
            }
        }

        [HttpGet("role/{roleId}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> GetByRole(int roleId)
        {
            if (!Enum.IsDefined(typeof(RoleEnum), roleId))
                return BadRequest(new { message = "Invalid role ID." });

            var users = await _userService.GetUsersByRoleAsync((RoleEnum)roleId);
            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] RegisterDto dto, [FromQuery] int roleId)
        {
            try
            {
                var result = await _userService.CreateUserAsync(dto, roleId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                await _userService.UpdateUserAsync(id, dto);
                return Ok(new { message = "User updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred", detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            try
            {
                await _userService.SoftDeleteUserAsync(id);
                return Ok(new { message = "User soft deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred", detail = ex.Message });
            }
        }

        [HttpPatch("{id}/block")]
        public async Task<IActionResult> BlockUnblock(int id, [FromBody] BlockUserDto dto)
        {
            try
            {
                await _userService.ToggleBlockUserAsync(id, dto.Block);
                return Ok(new { message = dto.Block ? "User blocked" : "User unblocked" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error", detail = ex.Message });
            }
        }
    }
}
