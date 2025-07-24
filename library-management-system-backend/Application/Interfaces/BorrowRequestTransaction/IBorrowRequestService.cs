using library_management_system_backend.Application.DTOs.BorrowRequestTransaction;

namespace library_management_system_backend.Application.Interfaces
{
    public interface IBorrowRequestService
    {
        Task<IEnumerable<BorrowRequestDto>> GetAllAsync(string? status = null);
        Task<IEnumerable<BorrowRequestDto>> GetPendingAsync(int? userId = null);
        Task CreateAsync(int userId, CreateBorrowRequestDto dto);
        Task ApproveAsync(int requestId, int approverId);
        Task RejectAsync(int requestId, int approverId, string? remarks);
    }
}