namespace library_management_system_backend.Application.Interfaces.Books
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<Book?> GetByIdAsync(int id);
        Task AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(int id);
        Task<bool> GenreExistsAsync(int genreId);

    }

}
