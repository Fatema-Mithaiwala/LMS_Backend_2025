namespace library_management_system_backend.Domain.Entities
{
    public class BorrowRequest
    {
        public int BorrowRequestId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
        public DateTime? DueDate { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Remarks { get; set; }

        public User User { get; set; }
        public Book Book { get; set; }
        public User? Approver { get; set; }
        public BorrowTransaction? BorrowTransaction { get; set; }
    }
}
