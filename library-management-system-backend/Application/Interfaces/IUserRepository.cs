using library_management_system_backend.Domain.Entities;

namespace library_management_system_backend.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId);
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateUserAsync(User user);
    }
}
