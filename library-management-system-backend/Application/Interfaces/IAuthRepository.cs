using library_management_system_backend.Domain.Entities;

namespace library_management_system_backend.Application.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task<bool> IsValidRoleId(int roleId);
        Task<Role> GetRoleByIdAsync(int roleId);


    }
}
