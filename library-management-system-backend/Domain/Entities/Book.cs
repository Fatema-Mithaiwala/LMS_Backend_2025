using library_management_system_backend.Domain.Entities;

public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int GenreId { get; set; }
    public int TotalCopies { get; set; } = 1;
    public int AvailableCopies { get; set; } = 1;
    public string? CoverImageUrl { get; set; }

    public Genre Genre { get; set; }
    public ICollection<BorrowRequest> BorrowRequests { get; set; }
    public ICollection<BorrowTransaction> BorrowTransactions { get; set; }
    public ICollection<ReturnRequest> ReturnRequests { get; set; }
    public ICollection<Wishlist> Wishlist { get; set; }
    public ICollection<Notification> Notifications { get; set; }
}
