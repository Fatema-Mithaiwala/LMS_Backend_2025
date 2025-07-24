using library_management_system_backend.Domain.Entities;

namespace library_management_system_backend.Application.Interfaces.Notifications
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, bool unreadOnly = false, int page = 1, int pageSize = 10);
        Task<Notification?> GetByIdAsync(int notificationId);
        Task<bool> ExistsAsync(int userId, int bookId, string notificationType);
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        
    }
}
