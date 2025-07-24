using Microsoft.EntityFrameworkCore;
using library_management_system_backend.Application.DTOs.BorrowTransaction;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management_system_backend.Application.DTOs.BorrowTransaction.library_management_system_backend.Application.DTOs.BorrowTransaction;
using library_management_system_backend.Application.Interfaces.BorrowTransactions;

namespace library_management_system_backend.Application.Services
{
    public class BorrowTransactionService : IBorrowTransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepo;

        public BorrowTransactionService(ApplicationDbContext context, IUserRepository userRepo)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public async Task<IEnumerable<BorrowTransactionDto>> GetBorrowTransactionsAsync(int? userId, string? returnDate, bool activeOnly)
        {
            try
            {
                var query = _context.BorrowTransactions
                    .Include(bt => bt.User).ThenInclude(u => u.Role)
                    .Include(bt => bt.Book)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Where(bt => bt.UserId == userId.Value);
                }

                if (activeOnly || returnDate == "null")
                {
                    query = query.Where(bt => bt.ReturnDate == null);
                }
                else if (!string.IsNullOrEmpty(returnDate))
                {
                    if (DateTime.TryParse(returnDate, out var parsedDate))
                    {
                        query = query.Where(bt => bt.ReturnDate == parsedDate);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid returnDate format.");
                    }
                }

                var transactions = await query.ToListAsync();

                return transactions.Select(bt => new BorrowTransactionDto
                {
                    TransactionId = bt.TransactionId,
                    BorrowRequestId = bt.BorrowRequestId,
                    UserId = bt.UserId,
                    UserName = bt.User?.FullName ?? "Unknown",
                    BookId = bt.BookId,
                    BookTitle = bt.Book?.Title ?? "Unknown",
                    BorrowDate = bt.BorrowDate,
                    DueDate = bt.DueDate,
                    ReturnDate = bt.ReturnDate,
                    PenaltyAmount = bt.PenaltyAmount,
                    Notes = bt.Notes ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch borrow transactions: {ex.Message}", ex);
            }
        }

        public async Task<BorrowTransactionDto?> GetByIdAsync(int transactionId)
        {
            try
            {
                var transaction = await _context.BorrowTransactions
                    .Include(bt => bt.User).ThenInclude(u => u.Role)
                    .Include(bt => bt.Book)
                    .FirstOrDefaultAsync(bt => bt.TransactionId == transactionId);

                if (transaction == null)
                    return null;

                return new BorrowTransactionDto
                {
                    TransactionId = transaction.TransactionId,
                    BorrowRequestId = transaction.BorrowRequestId,
                    UserId = transaction.UserId,
                    UserName = transaction.User?.FullName ?? "Unknown",
                    BookId = transaction.BookId,
                    BookTitle = transaction.Book?.Title ?? "Unknown",
                    BorrowDate = transaction.BorrowDate,
                    DueDate = transaction.DueDate,
                    ReturnDate = transaction.ReturnDate,
                    PenaltyAmount = transaction.PenaltyAmount,
                    Notes = transaction.Notes ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch transaction with ID {transactionId}: {ex.Message}", ex);
            }
        }
    }
}