namespace library_management_system_backend.Domain.Entities
{
    public class BorrowTransaction
    {
        public int TransactionId { get; set; }
        public int BorrowRequestId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal PenaltyAmount { get; set; } = 0;
        public string? Notes { get; set; }

        public User User { get; set; }
        public Book Book { get; set; }
        public BorrowRequest BorrowRequest { get; set; }
        public ReturnRequest? ReturnRequest { get; set; }
    }
}
