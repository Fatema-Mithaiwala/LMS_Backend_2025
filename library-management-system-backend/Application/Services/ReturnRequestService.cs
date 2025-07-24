using Microsoft.EntityFrameworkCore;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Application.Interfaces.Books;
using library_management_system_backend.Domain.Entities;
using library_management_system_backend.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management_system_backend.Application.DTOs.ReturnRequestTransaction;
using library_management_system_backend.Application.Interfaces.ReturnRequestTransaction;
using library_management_system_backend.Application.Interfaces.Notifications;

namespace library_management_system_backend.Application.Services
{
    public class ReturnRequestService : IReturnRequestService
    {
        private readonly IReturnRequestRepository _returnRequestRepo;
        private readonly IBookRepository _bookRepo;
        private readonly IUserRepository _userRepo;
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ReturnRequestService(
            IReturnRequestRepository returnRequestRepo,
            IBookRepository bookRepo,
            IUserRepository userRepo,
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _returnRequestRepo = returnRequestRepo ?? throw new ArgumentNullException(nameof(returnRequestRepo));
            _bookRepo = bookRepo ?? throw new ArgumentNullException(nameof(bookRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<IEnumerable<ReturnRequestDto>> GetAllAsync(string? status = null)
        {
            var requests = await _returnRequestRepo.GetAllAsync(status);
            var userIds = requests.Select(rr => rr.UserId).Distinct().ToList();
            var activeBorrowCounts = await _context.BorrowTransactions
                .Where(bt => userIds.Contains(bt.UserId) && bt.ReturnDate == null)
                .GroupBy(bt => bt.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.UserId, g => g.Count);

            return requests.Select(rr => new ReturnRequestDto
            {
                ReturnRequestId = rr.ReturnRequestId,
                BookId = rr.BookId,
                BookTitle = rr.Book?.Title ?? "Unknown",
                TransactionId = rr.TransactionId,
                UserId = rr.UserId,
                UserName = rr.User?.FullName ?? "Unknown",
                ReturnDate = rr.ReturnDate,
                Status = rr.Status ?? "Unknown",
                ProcessedBy = rr.ProcessedBy,
                ProcessedAt = rr.ProcessedAt,
                UserActiveBorrows = activeBorrowCounts.GetValueOrDefault(rr.UserId, 0)
            });
        }

        public async Task<IEnumerable<ReturnRequestDto>> GetPendingAsync(int? userId = null)
        {
            var requests = await _returnRequestRepo.GetAllPendingAsync();
            if (userId.HasValue)
            {
                requests = requests.Where(rr => rr.UserId == userId.Value).ToList();
            }

            var userIds = requests.Select(rr => rr.UserId).Distinct().ToList();
            var activeBorrowCounts = await _context.BorrowTransactions
                .Where(bt => userIds.Contains(bt.UserId) && bt.ReturnDate == null)
                .GroupBy(bt => bt.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.UserId, g => g.Count);

            return requests.Select(rr => new ReturnRequestDto
            {
                ReturnRequestId = rr.ReturnRequestId,
                BookId = rr.BookId,
                BookTitle = rr.Book?.Title ?? "Unknown",
                TransactionId = rr.TransactionId,
                UserId = rr.UserId,
                UserName = rr.User?.FullName ?? "Unknown",
                ReturnDate = rr.ReturnDate,
                Status = rr.Status ?? "Unknown",
                ProcessedBy = rr.ProcessedBy,
                ProcessedAt = rr.ProcessedAt,
                UserActiveBorrows = activeBorrowCounts.GetValueOrDefault(rr.UserId, 0)
            });
        }

        public async Task CreateAsync(int userId, CreateReturnRequestDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await _userRepo.GetUserByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");

            if (user.Role == null || string.IsNullOrEmpty(user.Role.RoleName) || user.IsBlocked || user.Role.RoleName != "Student")
                throw new UnauthorizedAccessException("User is blocked, has an invalid role, or is not a student.");

            var transaction = await _context.BorrowTransactions
                .Include(bt => bt.Book)
                .Include(bt => bt.User)
                .FirstOrDefaultAsync(bt => bt.TransactionId == dto.TransactionId && bt.UserId == userId)
                ?? throw new InvalidOperationException($"Transaction ID {dto.TransactionId} not found.");

            if (transaction.BookId != dto.BookId)
                throw new InvalidOperationException("Book ID does not match the transaction.");

            if (transaction.ReturnDate != null)
                throw new InvalidOperationException("This transaction has already been returned.");

            var existingRequest = await _context.ReturnRequests
                .Where(rr => rr.TransactionId == dto.TransactionId)
                .OrderByDescending(rr => rr.ProcessedAt) 
                .FirstOrDefaultAsync();

            if (existingRequest != null && existingRequest.Status == "Pending")
                throw new InvalidOperationException("A pending return request already exists for this transaction.");

            var returnRequest = new ReturnRequest
            {
                BookId = dto.BookId,
                TransactionId = dto.TransactionId,
                UserId = userId,
                ReturnDate = DateTime.UtcNow,
                Status = "Pending",
                User = user,
                Book = transaction.Book,
                BorrowTransaction = transaction
            };

            await _returnRequestRepo.AddAsync(returnRequest);
        }

        public async Task ApproveAsync(int requestId, int processorId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var returnRequest = await _returnRequestRepo.GetByIdAsync(requestId)
                    ?? throw new InvalidOperationException($"Return request {requestId} not found.");

                if (string.IsNullOrEmpty(returnRequest.Status) || returnRequest.Status != "Pending")
                    throw new InvalidOperationException($"Return request {requestId} is in '{returnRequest.Status}' status. Only pending requests can be approved.");

                var processor = await _userRepo.GetUserByIdAsync(processorId)
                    ?? throw new UnauthorizedAccessException("Processor not found.");

                if (processor.Role == null || string.IsNullOrEmpty(processor.Role.RoleName) || (processor.Role.RoleName != "Librarian" && processor.Role.RoleName != "Admin"))
                    throw new UnauthorizedAccessException("Processor has an invalid role or is not a librarian or admin.");

                var borrowTransaction = await _context.BorrowTransactions
                    .FirstOrDefaultAsync(bt => bt.TransactionId == returnRequest.TransactionId)
                    ?? throw new InvalidOperationException($"No transaction found for return request {requestId}.");

                if (borrowTransaction.ReturnDate != null)
                    throw new InvalidOperationException("This transaction has already been returned.");

                var book = await _bookRepo.GetByIdAsync(returnRequest.BookId)
                    ?? throw new InvalidOperationException($"Book not found for return request {requestId}.");

                var now = DateTime.UtcNow;

                if (now < borrowTransaction.BorrowDate)
                    throw new InvalidOperationException("Return date cannot be earlier than borrow date.");

                returnRequest.Status = "Approved";
                returnRequest.ProcessedBy = processorId;
                returnRequest.ProcessedAt = now;

                borrowTransaction.ReturnDate = now;
                book.AvailableCopies++;

                var wishlistUsers = await _context.WishLists
                    .Where(w => w.BookId == book.BookId && !w.IsNotified)
                    .Include(w => w.User)
                    .ToListAsync();

                foreach (var wishlist in wishlistUsers)
                {
                    try
                    {
                        await _notificationService.CreateBookAvailabilityNotificationAsync(book.BookId, wishlist.UserId);
                        wishlist.IsNotified = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to notify user {wishlist.UserId}: {ex.Message}");
                    }
                }

                _context.ReturnRequests.Update(returnRequest);
                _context.BorrowTransactions.Update(borrowTransaction);
                _context.Books.Update(book);

                await _context.SaveChangesAsync();

                await _notificationService.CreateReturnRequestApprovedNotificationAsync(
                    returnRequest.ReturnRequestId,
                    returnRequest.UserId,
                    returnRequest.BookId);

                await dbTransaction.CommitAsync();
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task RejectAsync(int requestId, int processorId)
        {
            var returnRequest = await _returnRequestRepo.GetByIdAsync(requestId)
                ?? throw new InvalidOperationException($"Return request {requestId} not found.");

            if (string.IsNullOrEmpty(returnRequest.Status) || returnRequest.Status != "Pending")
                throw new InvalidOperationException($"Return request {requestId} cannot be rejected because it is in '{returnRequest.Status}' status. Only pending requests can be rejected.");

            var processor = await _userRepo.GetUserByIdAsync(processorId)
                ?? throw new UnauthorizedAccessException("Processor not found.");

            if (processor.Role == null || string.IsNullOrEmpty(processor.Role.RoleName) || (processor.Role.RoleName != "Librarian" && processor.Role.RoleName != "Admin"))
                throw new UnauthorizedAccessException("Processor has an invalid role or is not a librarian or admin.");

            returnRequest.Status = "Rejected";
            returnRequest.ProcessedBy = processorId;
            returnRequest.ProcessedAt = DateTime.UtcNow;

            _context.ReturnRequests.Update(returnRequest);
            await _context.SaveChangesAsync();

            await _notificationService.CreateReturnRequestRejectedNotificationAsync(
                returnRequest.ReturnRequestId,
                returnRequest.UserId,
                returnRequest.BookId,
                returnRequest.ConditionRemarks);
        }
    }
}
