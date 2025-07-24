using library_management_system_backend.Application.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library_management_system_backend.Application.Interfaces.Notifications
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId, bool unreadOnly = false, int page = 1, int pageSize = 10);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task CreateBookAvailabilityNotificationAsync(int bookId, int userId);
        Task CreateBorrowRequestApprovedNotificationAsync(int borrowRequestId, int userId, int bookId, DateTime dueDate);
        Task CreateBorrowRequestRejectedNotificationAsync(int borrowRequestId, int userId, int bookId, string? remarks);
        Task CreateReturnRequestApprovedNotificationAsync(int returnRequestId, int userId, int bookId);
        Task CreateReturnRequestRejectedNotificationAsync(int returnRequestId, int userId, int bookId, string? remarks);
        Task SendBorrowRequestApprovedEmailAsync(string email, string bookTitle, DateTime dueDate);
        Task SendBorrowRequestRejectedEmailAsync(string email, string bookTitle, string? remarks);
        Task SendReturnRequestApprovedEmailAsync(string email, string bookTitle);
        Task SendReturnRequestRejectedEmailAsync(string email, string bookTitle, string? remarks);
    }
}