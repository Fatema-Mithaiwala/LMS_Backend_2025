namespace library_management_system_backend.Application.DTOs.ReturnRequestTransaction
{
    public class ReturnRequestDto
    {
        public int ReturnRequestId { get; set; }
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime ReturnDate { get; set; }
        public string ConditionRemarks { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string Status { get; set; }
        public int? ProcessedBy { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public int UserActiveBorrows { get; set; }
    }

}
