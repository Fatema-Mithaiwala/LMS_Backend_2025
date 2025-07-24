using library_management_system_backend.Application.DTOs.Notifications;
using library_management_system_backend.Application.Exceptions;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Application.Interfaces.Books;
using library_management_system_backend.Application.Interfaces.Notifications;
using library_management_system_backend.Domain.Entities;


namespace library_management_system_backend.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IEmailService _emailService;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IBookRepository bookRepository,
            IEmailService emailService)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId, bool unreadOnly = false, int page = 1, int pageSize = 10)
        {
            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            var notifications = await _notificationRepository.GetByUserIdAsync(userId, unreadOnly, page, pageSize);
            Console.WriteLine($"Retrieved {notifications.Count()} notifications for user {userId}");
            return notifications.Select(n => MapToDto(n));
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId)
                ?? throw new NotFoundException($"Notification with ID {notificationId} not found.");

            if (notification.UserId != userId)
                throw new UnauthorizedAccessException("Not authorized to modify this notification.");

            if (notification.IsRead)
                return;

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task CreateBookAvailabilityNotificationAsync(int bookId, int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            var book = await _bookRepository.GetByIdAsync(bookId)
                ?? throw new NotFoundException($"Book with ID {bookId} not found.");

            if (await _notificationRepository.ExistsAsync(userId, bookId, "BookAvailable"))
            {
                Console.WriteLine($"Notification already exists for user {userId} and book {bookId}");
                return;
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = "Book Available",
                Message = $"The book '{book.Title}' is now available for borrowing.",
                NotificationType = "BookAvailable",
                RelatedBookId = bookId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                User = user,
                RelatedBook = book
            };

            await _notificationRepository.AddAsync(notification);
            Console.WriteLine($"Notification created for user {userId}, book {bookId}");

            if (!string.IsNullOrEmpty(user.Email))
            {
                await SendAvailabilityEmailAsync(user.Email, book.Title);
                Console.WriteLine($"Email dispatched to {user.Email} for book {book.Title}");
            }
            else
            {
                Console.WriteLine($"No email address found for user {userId}");
            }
        }

        public async Task CreateBorrowRequestApprovedNotificationAsync(int borrowRequestId, int userId, int bookId, DateTime dueDate)
        {
            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            var book = await _bookRepository.GetByIdAsync(bookId)
                ?? throw new NotFoundException($"Book with ID {bookId} not found.");

            if (await _notificationRepository.ExistsAsync(userId, borrowRequestId, "BorrowRequestApproved"))
            {
                Console.WriteLine($"Notification already exists for user {userId} and borrow request {borrowRequestId}");
                return; 
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = "Borrow Request Approved",
                Message = $"Your borrow request for the book '{book.Title}' has been approved. Due Date: {dueDate:yyyy-MM-dd}",
                NotificationType = "BorrowRequestApproved",
                RelatedBookId = bookId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                User = user,
                RelatedBook = book
            };

            await _notificationRepository.AddAsync(notification);
            Console.WriteLine($"Notification created for user {userId}, borrow request {borrowRequestId}");

            if (!string.IsNullOrEmpty(user.Email))
            {
                await SendBorrowRequestApprovedEmailAsync(user.Email, book.Title, dueDate);
                Console.WriteLine($"Email dispatched to {user.Email} for borrow request approval of book {book.Title}");
            }
            else
            {
                Console.WriteLine($"No email address found for user {userId}");
            }
        }

        public async Task CreateBorrowRequestRejectedNotificationAsync(int borrowRequestId, int userId, int bookId, string? remarks)
        {
            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            var book = await _bookRepository.GetByIdAsync(bookId)
                ?? throw new NotFoundException($"Book with ID {bookId} not found.");

            if (await _notificationRepository.ExistsAsync(userId, borrowRequestId, "BorrowRequestRejected"))
            {
                Console.WriteLine($"Notification already exists for user {userId} and borrow request {borrowRequestId}");
                return;
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = "Borrow Request Rejected",
                Message = $"Your borrow request for the book '{book.Title}' has been rejected." + (remarks != null ? $" Reason: {remarks}" : ""),
                NotificationType = "BorrowRequestRejected",
                RelatedBookId = bookId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                User = user,
                RelatedBook = book
            };

            await _notificationRepository.AddAsync(notification);
            Console.WriteLine($"Notification created for user {userId}, borrow request {borrowRequestId}");

            if (!string.IsNullOrEmpty(user.Email))
            {
                await SendBorrowRequestRejectedEmailAsync(user.Email, book.Title, remarks);
                Console.WriteLine($"Email dispatched to {user.Email} for borrow request rejection of book {book.Title}");
            }
            else
            {
                Console.WriteLine($"No email address found for user {userId}");
            }
        }

        public async Task CreateReturnRequestApprovedNotificationAsync(int returnRequestId, int userId, int bookId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            var book = await _bookRepository.GetByIdAsync(bookId)
                ?? throw new NotFoundException($"Book with ID {bookId} not found.");

            if (await _notificationRepository.ExistsAsync(userId, returnRequestId, "ReturnRequestApproved"))
            {
                Console.WriteLine($"Notification already exists for user {userId} and return request {returnRequestId}");
                return; 
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = "Return Request Approved",
                Message = $"Your return request for the book '{book.Title}' has been approved.",
                NotificationType = "ReturnRequestApproved",
                RelatedBookId = bookId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                User = user,
                RelatedBook = book
            };

            await _notificationRepository.AddAsync(notification);
            Console.WriteLine($"Notification created for user {userId}, return request {returnRequestId}");

            if (!string.IsNullOrEmpty(user.Email))
            {
                await SendReturnRequestApprovedEmailAsync(user.Email, book.Title);
                Console.WriteLine($"Email dispatched to {user.Email} for return request approval of book {book.Title}");
            }
            else
            {
                Console.WriteLine($"No email address found for user {userId}");
            }
        }

        public async Task CreateReturnRequestRejectedNotificationAsync(int returnRequestId, int userId, int bookId, string? remarks)
        {
            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            var book = await _bookRepository.GetByIdAsync(bookId)
                ?? throw new NotFoundException($"Book with ID {bookId} not found.");

            if (await _notificationRepository.ExistsAsync(userId, returnRequestId, "ReturnRequestRejected"))
            {
                Console.WriteLine($"Notification already exists for user {userId} and return request {returnRequestId}");
                return; 
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = "Return Request Rejected",
                Message = $"Your return request for the book '{book.Title}' has been rejected." + (remarks != null ? $" Reason: {remarks}" : ""),
                NotificationType = "ReturnRequestRejected",
                RelatedBookId = bookId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                User = user,
                RelatedBook = book
            };

            await _notificationRepository.AddAsync(notification);
            Console.WriteLine($"Notification created for user {userId}, return request {returnRequestId}");

            if (!string.IsNullOrEmpty(user.Email))
            {
                await SendReturnRequestRejectedEmailAsync(user.Email, book.Title, remarks);
                Console.WriteLine($"Email dispatched to {user.Email} for return request rejection of book {book.Title}");
            }
            else
            {
                Console.WriteLine($"No email address found for user {userId}");
            }
        }

        private async Task SendAvailabilityEmailAsync(string email, string bookTitle)
        {
            try
            {
                await _emailService.SendBookAvailabilityEmailAsync(email, bookTitle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send availability email to {email}: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        public async Task SendBorrowRequestApprovedEmailAsync(string email, string bookTitle, DateTime dueDate)
        {
            try
            {
                await _emailService.SendBorrowRequestApprovedEmailAsync(email, bookTitle, dueDate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send borrow request approved email to {email}: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        public async Task SendBorrowRequestRejectedEmailAsync(string email, string bookTitle, string? remarks)
        {
            try
            {
                await _emailService.SendBorrowRequestRejectedEmailAsync(email, bookTitle, remarks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send borrow request rejected email to {email}: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        public async Task SendReturnRequestApprovedEmailAsync(string email, string bookTitle)
        {
            try
            {
                await _emailService.SendReturnRequestApprovedEmailAsync(email, bookTitle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send return request approved email to {email}: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        public async Task SendReturnRequestRejectedEmailAsync(string email, string bookTitle, string? remarks)
        {
            try
            {
                await _emailService.SendReturnRequestRejectedEmailAsync(email, bookTitle, remarks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send return request rejected email to {email}: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                NotificationType = notification.NotificationType,
                RelatedBookId = notification.RelatedBookId,
                RelatedBookTitle = notification.RelatedBook?.Title,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}