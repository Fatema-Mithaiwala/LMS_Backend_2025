public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int GenreId { get; set; }
    public int TotalCopies { get; set; }
    public IFormFile? CoverImage { get; set; } 
}
