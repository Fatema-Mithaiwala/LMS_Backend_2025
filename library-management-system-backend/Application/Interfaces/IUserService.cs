using library_management_system_backend.Application.DTOs;
using library_management_system_backend.Domain.Enums;

namespace library_management_system_backend.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(RoleEnum role);
        Task SoftDeleteUserAsync(int userId);
        Task UpdateUserAsync(int userId, UpdateUserDto dto);
        Task<object> CreateUserAsync(RegisterDto dto, int roleId);
        Task<UserDto> GetUserByIdAsync(int userId);

        Task ToggleBlockUserAsync(int userId, bool block);

    }
}
