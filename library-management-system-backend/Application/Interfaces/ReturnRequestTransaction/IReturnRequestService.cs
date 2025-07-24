using library_management_system_backend.Application.DTOs.ReturnRequestTransaction;

namespace library_management_system_backend.Application.Interfaces.ReturnRequestTransaction
{
    public interface IReturnRequestService
    {
        Task<IEnumerable<ReturnRequestDto>> GetAllAsync(string? status = null);
        Task<IEnumerable<ReturnRequestDto>> GetPendingAsync(int? userId = null);
        Task CreateAsync(int userId, CreateReturnRequestDto dto);
        Task ApproveAsync(int requestId, int processorId);
        Task RejectAsync(int requestId, int processorId);
    }
}
