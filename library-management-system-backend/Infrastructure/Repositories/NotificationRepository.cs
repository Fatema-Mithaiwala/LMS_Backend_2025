using Microsoft.EntityFrameworkCore;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Domain.Entities;
using library_management_system_backend.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management_system_backend.Application.Interfaces.Notifications;

namespace library_management_system_backend.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, bool unreadOnly = false, int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Invalid pagination parameters.");

            var query = _context.Notifications
                .Include(n => n.User)
                .Include(n => n.RelatedBook)
                .Where(n => n.UserId == userId);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int notificationId)
        {
            return await _context.Notifications
                .Include(n => n.User)
                .Include(n => n.RelatedBook)
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        }

        public async Task<bool> ExistsAsync(int userId, int bookId, string notificationType)
        {
            return await _context.Notifications
                .AnyAsync(n => n.UserId == userId && n.RelatedBookId == bookId && n.NotificationType == notificationType && !n.IsRead);
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }
    }
}