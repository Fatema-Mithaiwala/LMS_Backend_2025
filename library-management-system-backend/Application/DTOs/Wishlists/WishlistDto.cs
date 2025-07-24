namespace library_management_system_backend.Application.DTOs.Wishlists
{
    public class WishlistDto
    {
        public int WishlistId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        public bool IsNotified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
