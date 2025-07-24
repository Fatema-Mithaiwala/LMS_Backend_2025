using library_management_system_backend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library_management_system_backend.Application.Interfaces
{
    public interface IWishlistRepository
    {
        Task<IEnumerable<Wishlist>> GetAllAsync();
        Task<IEnumerable<Wishlist>> GetByUserIdAsync(int userId);
        Task<Wishlist?> GetByIdAsync(int wishlistId);
        Task<bool> ExistsAsync(int userId, int bookId);
        Task<IEnumerable<Wishlist>> GetByBookIdAsync(int bookId, bool notNotifiedOnly = false);
        Task AddAsync(Wishlist wishlist);
        Task DeleteAsync(Wishlist wishlist);
        Task UpdateAsync(Wishlist wishlist);
    }
}