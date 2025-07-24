namespace library_management_system_backend.Domain.Entities
{
    public class ReturnRequest
    {
        public int ReturnRequestId { get; set; }
        public int BookId { get; set; }
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
        public string ConditionRemarks { get; set; } = string.Empty;
        public decimal PenaltyAmount { get; set; } = 0;
        public string Status { get; set; } = "Pending";
        public int? ProcessedBy { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public User User { get; set; }
        public Book Book { get; set; }
        public BorrowTransaction BorrowTransaction { get; set; }
    }
}
