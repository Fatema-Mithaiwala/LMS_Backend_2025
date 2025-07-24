using Microsoft.EntityFrameworkCore;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Domain.Entities;
using library_management_system_backend.Infrastructure.Data;


namespace library_management_system_backend.Infrastructure.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;

        public WishlistRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Wishlist>> GetAllAsync()
        {
            return await _context.WishLists
                .Include(w => w.User)
                .Include(w => w.Book)
                .ToListAsync();
        }

        public async Task<IEnumerable<Wishlist>> GetByUserIdAsync(int userId)
        {
            return await _context.WishLists
                .Include(w => w.User)
                .Include(w => w.Book)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<Wishlist?> GetByIdAsync(int wishlistId)
        {
            return await _context.WishLists
                .Include(w => w.User)
                .Include(w => w.Book)
                .FirstOrDefaultAsync(w => w.WishlistId == wishlistId);
        }

        public async Task<bool> ExistsAsync(int userId, int bookId)
        {
            return await _context.WishLists
                .AnyAsync(w => w.UserId == userId && w.BookId == bookId);
        }

        public async Task<IEnumerable<Wishlist>> GetByBookIdAsync(int bookId, bool notNotifiedOnly = false)
        {
            var query = _context.WishLists
                .Include(w => w.User)
                .Include(w => w.Book)
                .Where(w => w.BookId == bookId);

            if (notNotifiedOnly)
                query = query.Where(w => !w.IsNotified);

            return await query.ToListAsync();
        }

        public async Task AddAsync(Wishlist wishlist)
        {
            await _context.WishLists.AddAsync(wishlist);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Wishlist wishlist)
        {
            _context.WishLists.Remove(wishlist);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Wishlist wishlist)
        {
            _context.WishLists.Update(wishlist);
            await _context.SaveChangesAsync();
        }
    }
}