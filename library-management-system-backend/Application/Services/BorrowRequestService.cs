using Microsoft.EntityFrameworkCore;
using library_management_system_backend.Application.DTOs.BorrowRequestTransaction;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Application.Interfaces.Books;
using library_management_system_backend.Domain.Entities;
using library_management_system_backend.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management_system_backend.Application.Interfaces.Notifications;

namespace library_management_system_backend.Application.Services
{
    public class BorrowRequestService : IBorrowRequestService
    {
        private readonly IBorrowRequestRepository _borrowRequestRepo;
        private readonly IBookRepository _bookRepo;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepo;
        private readonly ApplicationDbContext _context;
        private const int MAX_BORROWS = 3;
        private const int LOAN_PERIOD_DAYS = 14;

        public BorrowRequestService(
            IBorrowRequestRepository borrowRequestRepo,
            IBookRepository bookRepo,
            INotificationService notificationService,
            IUserRepository userRepo,
            ApplicationDbContext context)
        {
            _borrowRequestRepo = borrowRequestRepo ?? throw new ArgumentNullException(nameof(borrowRequestRepo));
            _bookRepo = bookRepo ?? throw new ArgumentNullException(nameof(bookRepo));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<BorrowRequestDto>> GetAllAsync(string? status = null)
        {
            var requests = await _borrowRequestRepo.GetAllAsync(status);
            var userIds = requests.Select(br => br.UserId).Distinct().ToList();
            var activeBorrowCounts = await _context.BorrowTransactions
                .Where(bt => userIds.Contains(bt.UserId) && bt.ReturnDate == null)
                .GroupBy(bt => bt.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.UserId, g => g.Count);

            return requests.Select(br => new BorrowRequestDto
            {
                BorrowRequestId = br.BorrowRequestId,
                UserId = br.UserId,
                UserName = br.User?.FullName ?? "Unknown",
                BookId = br.BookId,
                BookTitle = br.Book?.Title ?? "Unknown",
                RequestDate = br.RequestDate,
                Status = br.Status ?? "Unknown",
                ApproverId = br.ApprovedBy,
                ApprovedAt = br.ApprovedAt,
                Remarks = br.Remarks,
                UserActiveBorrows = activeBorrowCounts.GetValueOrDefault(br.UserId, 0)
            });
        }

        public async Task<IEnumerable<BorrowRequestDto>> GetPendingAsync(int? userId = null)
        {
            var requests = await _borrowRequestRepo.GetAllPendingAsync();
            if (userId.HasValue)
            {
                requests = requests.Where(br => br.UserId == userId.Value).ToList();
            }

            var userIds = requests.Select(r => r.UserId).Distinct().ToList();
            var activeBorrowCounts = await _context.BorrowTransactions
                .Where(bt => userIds.Contains(bt.UserId) && bt.ReturnDate == null)
                .GroupBy(bt => bt.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.UserId, g => g.Count);

            return requests.Select(br => new BorrowRequestDto
            {
                BorrowRequestId = br.BorrowRequestId,
                UserId = br.UserId,
                UserName = br.User?.FullName ?? "Unknown",
                BookId = br.BookId,
                BookTitle = br.Book?.Title ?? "Unknown",
                RequestDate = br.RequestDate,
                Status = br.Status ?? "Unknown",
                ApproverId = br.ApprovedBy,
                ApprovedAt = br.ApprovedAt,
                Remarks = br.Remarks,
                UserActiveBorrows = activeBorrowCounts.GetValueOrDefault(br.UserId, 0)
            });
        }

        public async Task CreateAsync(int userId, CreateBorrowRequestDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await _userRepo.GetUserByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");

            if (user.Role == null || string.IsNullOrEmpty(user.Role.RoleName) || user.IsBlocked || user.Role.RoleName != "Student")
                throw new UnauthorizedAccessException("User is blocked, has an invalid role, or is not a student.");

            var activeBorrow = await _context.BorrowTransactions
                .AnyAsync(bt => bt.UserId == userId && bt.BookId == dto.BookId && bt.ReturnDate == null);
            if (activeBorrow)
                throw new InvalidOperationException("You already have an active borrow for this book.");

            if (await _borrowRequestRepo.FindPendingAsync(userId, dto.BookId) != null)
                throw new InvalidOperationException("A pending request for this book already exists.");

            var activeBorrows = await _context.BorrowTransactions
                .CountAsync(bt => bt.UserId == userId && bt.ReturnDate == null);
            var pendingBorrows = await _borrowRequestRepo.CountPendingAsync(userId);
            if (activeBorrows + pendingBorrows >= MAX_BORROWS)
                throw new InvalidOperationException($"Maximum {MAX_BORROWS} active or pending borrows allowed.");

            var book = await _bookRepo.GetByIdAsync(dto.BookId)
                ?? throw new ArgumentException("Book not found.");
            if (book.AvailableCopies < 1)
                throw new InvalidOperationException("No copies available.");

            var borrowRequest = new BorrowRequest
            {
                UserId = userId,
                BookId = dto.BookId,
                RequestDate = DateTime.UtcNow,
                Status = "Pending",
                User = user,
                Book = book
            };

            await _borrowRequestRepo.AddAsync(borrowRequest);
        }

        public async Task ApproveAsync(int requestId, int approverId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var borrowRequest = await _borrowRequestRepo.GetByIdAsync(requestId)
                    ?? throw new KeyNotFoundException("Borrow request not found.");
                if (string.IsNullOrEmpty(borrowRequest.Status) || borrowRequest.Status != "Pending")
                    throw new InvalidOperationException("Only pending requests can be approved.");

                var approver = await _userRepo.GetUserByIdAsync(approverId)
                    ?? throw new UnauthorizedAccessException("Approver not found.");

                if (approver.Role == null || string.IsNullOrEmpty(approver.Role.RoleName) || (approver.Role.RoleName != "Librarian" && approver.Role.RoleName != "Admin"))
                    throw new UnauthorizedAccessException("Approver has an invalid role or is not a librarian or admin.");

                var activeBorrows = await _context.BorrowTransactions
                    .CountAsync(bt => bt.UserId == borrowRequest.UserId && bt.ReturnDate == null);
                if (activeBorrows >= MAX_BORROWS)
                    throw new InvalidOperationException($"User already has {MAX_BORROWS} active borrows.");

                bool stillBorrowed = await _context.BorrowTransactions
    .AnyAsync(bt => bt.UserId == borrowRequest.UserId &&
                    bt.BookId == borrowRequest.BookId &&
                    bt.ReturnDate == null);

                if (stillBorrowed)
                    throw new InvalidOperationException(
                        "User already has an outstanding copy of this book.");


                var book = await _bookRepo.GetByIdAsync(borrowRequest.BookId)
                    ?? throw new ArgumentException("Book not found.");
                if (book.AvailableCopies < 1)
                    throw new InvalidOperationException("No copies available.");

                borrowRequest.Status = "Approved";
                borrowRequest.ApprovedBy = approverId;
                borrowRequest.ApprovedAt = DateTime.UtcNow;
                borrowRequest.DueDate = DateTime.UtcNow.AddDays(LOAN_PERIOD_DAYS);
                borrowRequest.Approver = approver;

                book.AvailableCopies--;

                var borrowTransaction = new BorrowTransaction
                {
                    BorrowRequestId = borrowRequest.BorrowRequestId,
                    UserId = borrowRequest.UserId,
                    BookId = borrowRequest.BookId,
                    BorrowDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(LOAN_PERIOD_DAYS),
                    ReturnDate = null,
                    PenaltyAmount = 0,
                    Notes = "Book issued upon approval",
                    User = borrowRequest.User,
                    Book = book
                };
                _context.BorrowTransactions.Add(borrowTransaction);

                _context.Books.Update(book);
                _context.BorrowRequests.Update(borrowRequest);

                await _context.SaveChangesAsync();

                await _notificationService.CreateBorrowRequestApprovedNotificationAsync(
                    borrowRequest.BorrowRequestId,
                    borrowRequest.UserId,
                    borrowRequest.BookId,
                    borrowRequest.DueDate.Value);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RejectAsync(int requestId, int approverId, string? remarks)
        {
            var borrowRequest = await _borrowRequestRepo.GetByIdAsync(requestId)
                ?? throw new KeyNotFoundException($"Borrow request {requestId} not found.");
            if (string.IsNullOrEmpty(borrowRequest.Status) || borrowRequest.Status != "Pending")
                throw new InvalidOperationException($"Borrow request {requestId} cannot be rejected because it is in '{borrowRequest.Status ?? "Unknown"}' status. Only pending requests can be rejected.");

            var approver = await _userRepo.GetUserByIdAsync(approverId)
                ?? throw new UnauthorizedAccessException("Approver not found.");

            if (approver.Role == null || string.IsNullOrEmpty(approver.Role.RoleName) || (approver.Role.RoleName != "Librarian" && approver.Role.RoleName != "Admin"))
                throw new UnauthorizedAccessException("Approver has an invalid role or is not a librarian or admin.");

            borrowRequest.Status = "Rejected";
            borrowRequest.ApprovedBy = approverId;
            borrowRequest.ApprovedAt = DateTime.UtcNow;
            borrowRequest.Remarks = remarks;
            borrowRequest.Approver = approver;

            _context.BorrowRequests.Update(borrowRequest);
            await _context.SaveChangesAsync();

            await _notificationService.CreateBorrowRequestRejectedNotificationAsync(
                borrowRequest.BorrowRequestId,
                borrowRequest.UserId,
                borrowRequest.BookId,
                remarks);
        }
    }
}