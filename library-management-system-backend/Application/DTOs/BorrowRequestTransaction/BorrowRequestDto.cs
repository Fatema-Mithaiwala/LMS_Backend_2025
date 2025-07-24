namespace library_management_system_backend.Application.DTOs.BorrowRequestTransaction
{
    public class BorrowRequestDto
    {
        public int BorrowRequestId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        public DateTime RequestDate { get; set; }
        public string? Status { get; set; }
        public int? ApproverId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Remarks { get; set; }
        public int UserActiveBorrows { get; set; }
    }
}