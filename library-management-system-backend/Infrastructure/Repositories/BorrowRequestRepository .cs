using Microsoft.EntityFrameworkCore;
using library_management_system_backend.Domain.Entities;
using library_management_system_backend.Infrastructure.Data;
using library_management_system_backend.Application.Interfaces;

namespace library_management_system_backend.Infrastructure.Repositories
{
    public class BorrowRequestRepository : IBorrowRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public BorrowRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BorrowRequest>> GetAllAsync(string? status = null)
        {
            var query = _context.BorrowRequests
                .Include(br => br.User)
                .Include(br => br.Book)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(br => br.Status == status);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<BorrowRequest>> GetAllPendingAsync()
        {
            return await _context.BorrowRequests
                .Include(br => br.User)
                .Include(br => br.Book)
                .Where(br => br.Status == "Pending")
                .ToListAsync();
        }

        public async Task<BorrowRequest?> GetByIdAsync(int id)
        {
            return await _context.BorrowRequests
                .Include(br => br.User)
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.BorrowRequestId == id);
        }

        public async Task<BorrowRequest?> FindPendingAsync(int userId, int bookId)
        {
            return await _context.BorrowRequests
                .FirstOrDefaultAsync(br => br.UserId == userId && br.BookId == bookId && br.Status == "Pending");
        }

        public async Task AddAsync(BorrowRequest request)
        {
            _context.BorrowRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BorrowRequest request)
        {
            _context.BorrowRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountPendingAsync(int userId)
        {
            return await _context.BorrowRequests
                .CountAsync(br => br.UserId == userId && br.Status == "Pending");
        }
    }
}