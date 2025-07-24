namespace library_management_system_backend.Application.DTOs.BorrowTransaction
{
    namespace library_management_system_backend.Application.DTOs.BorrowTransaction
    {
        public class BorrowTransactionDto
        {
            public int TransactionId { get; set; }
            public int BorrowRequestId { get; set; }
            public int UserId { get; set; }
            public string? UserName { get; set; }
            public int BookId { get; set; }
            public string? BookTitle { get; set; }
            public DateTime BorrowDate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime? ReturnDate { get; set; }
            public Decimal? PenaltyAmount { get; set; }
            public string Notes { get; set; } = string.Empty;
        }
    }
}
