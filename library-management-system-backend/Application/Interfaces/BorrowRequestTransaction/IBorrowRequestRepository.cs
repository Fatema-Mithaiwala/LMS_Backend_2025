using library_management_system_backend.Domain.Entities;

namespace library_management_system_backend.Application.Interfaces
{
    public interface IBorrowRequestRepository
    {
        Task<IEnumerable<BorrowRequest>> GetAllAsync(string? status = null);
        Task<IEnumerable<BorrowRequest>> GetAllPendingAsync();
        Task<BorrowRequest?> GetByIdAsync(int id);
        Task<BorrowRequest?> FindPendingAsync(int userId, int bookId);
        Task AddAsync(BorrowRequest request);
        Task UpdateAsync(BorrowRequest request);
        Task<int> CountPendingAsync(int userId);
    }
}