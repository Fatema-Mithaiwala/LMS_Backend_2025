namespace library_management_system_backend.Domain.Entities
{
    public class User 
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;

        public bool IsBlocked { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Role Role { get; set; }
        public ICollection<BorrowRequest> BorrowRequests { get; set; }
        public ICollection<BorrowTransaction> BorrowTransactions { get; set; }
        public ICollection<ReturnRequest> ReturnRequests { get; set; }
        public ICollection<Wishlist> Wishlist { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
