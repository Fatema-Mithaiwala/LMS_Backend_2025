using library_management_system_backend.Application.DTOs.Books;

namespace library_management_system_backend.Application.Interfaces.Books
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllAsync();
        Task<BookDto> GetByIdAsync(int id);
        Task CreateAsync(CreateBookDto dto);
        Task<(bool Success, string ErrorMessage)> UpdateAsync(int id, UpdateBookDto dto);
        Task DeleteAsync(int id);
    }

}
