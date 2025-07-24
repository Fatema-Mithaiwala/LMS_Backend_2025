namespace library_management_system_backend.Domain.Entities
{
    public class Wishlist
    {
        public int WishlistId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public bool IsNotified { get; set; } = false;
        public User User { get; set; }
        public Book Book { get; set; }
    }
}
