using library_management_system_backend.Domain.Entities;

namespace library_management_system_backend.Application.Interfaces.ReturnRequestTransaction
{
    public interface IReturnRequestRepository
    {
        Task<IEnumerable<ReturnRequest>> GetAllAsync(string? status = null);
        Task<IEnumerable<ReturnRequest>> GetAllPendingAsync();
        Task<ReturnRequest?> GetByIdAsync(int id);
        Task AddAsync(ReturnRequest returnRequest);
        Task UpdateAsync(ReturnRequest returnRequest);
    }
}
