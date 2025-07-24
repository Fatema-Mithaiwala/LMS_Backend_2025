using library_management_system_backend.Application.DTOs.BorrowTransaction.library_management_system_backend.Application.DTOs.BorrowTransaction;

namespace library_management_system_backend.Application.Interfaces.BorrowTransactions
{
    public interface IBorrowTransactionService
    {
        Task<IEnumerable<BorrowTransactionDto>> GetBorrowTransactionsAsync(int? userId, string? returnDate, bool activeOnly);
        Task<BorrowTransactionDto?> GetByIdAsync(int transactionId);

    }
}
