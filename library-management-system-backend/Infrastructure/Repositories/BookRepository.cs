using library_management_system_backend.Application.Interfaces.Books;
using library_management_system_backend.Domain.Entities;
using library_management_system_backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace library_management_system_backend.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _context;

        public BookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Books
                .Select(b => new Book
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    Description = b.Description,
                    GenreId = b.GenreId,
                    Genre = new Genre { GenreName = b.Genre.GenreName },
                    TotalCopies = b.TotalCopies,
                    AvailableCopies = b.AvailableCopies,
                    CoverImageUrl = b.CoverImageUrl
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.Include(b => b.Genre).FirstOrDefaultAsync(b => b.BookId == id);
        }

        public async Task AddAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> GenreExistsAsync(int genreId)
        {
            return await _context.Genres.AnyAsync(g => g.GenreId == genreId);
        }

    }
}
