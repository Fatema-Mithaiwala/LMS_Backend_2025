using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Domain.Entities;
using library_management_system_backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId == roleId && !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
