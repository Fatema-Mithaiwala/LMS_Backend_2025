using library_management_system_backend.Application.DTOs.Books;
using library_management_system_backend.Application.Interfaces.Books;
using library_management_system_backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _repo;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public BookService(IBookRepository repo, IWebHostEnvironment env, ApplicationDbContext context)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<BookDto>> GetAllAsync()
        {
            var books = await _repo.GetAllAsync();
            return books.Select(b => new BookDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                Description = b.Description,
                GenreName = b.Genre.GenreName,
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
                CoverImageBase64 = b.CoverImageUrl
            });
        }

        public async Task<BookDto?> GetByIdAsync(int id)
        {
            var b = await _repo.GetByIdAsync(id);
            if (b == null) return null;
            return new BookDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                Description = b.Description,
                GenreName = b.Genre.GenreName,
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
                CoverImageBase64 = b.CoverImageUrl
            };
        }

        public async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var basePath = _env.ContentRootPath ?? Directory.GetCurrentDirectory();
            Console.WriteLine($"Base Path: {basePath}");
            var uploadsFolder = Path.Combine(basePath, "Uploads", "images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return "/Uploads/images/" + uniqueFileName;
        }

        public async Task CreateAsync(CreateBookDto dto)
        {
            if (dto.TotalCopies < 0)
            {
                Console.WriteLine("[CreateAsync] Validation Failed: Total copies cannot be negative.");
                throw new ArgumentException("Total copies cannot be negative.");
            }

            var imagePath = await SaveImageAsync(dto.CoverImage);
            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                ISBN = dto.ISBN,
                Description = dto.Description,
                GenreId = dto.GenreId,
                TotalCopies = dto.TotalCopies,
                AvailableCopies = dto.TotalCopies,
                CoverImageUrl = imagePath
            };
            await _repo.AddAsync(book);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(int id, UpdateBookDto dto)
        {
            var book = await _repo.GetByIdAsync(id);
            if (book == null)
            {
                Console.WriteLine($"[UpdateAsync] Error: Book not found for BookId: {id}");
                return (false, "Book not found.");
            }

            var activeBorrowsList = await _context.BorrowTransactions
                .Where(bt => bt.BookId == id && bt.ReturnDate == null)
                .ToListAsync();
            var activeBorrows = activeBorrowsList.Count;

            Console.WriteLine($"[UpdateAsync] BookId: {id}, Current TotalCopies: {book.TotalCopies}, Current AvailableCopies: {book.AvailableCopies}");
            Console.WriteLine($"[UpdateAsync] Active Borrows: {activeBorrows}, Transaction IDs: {string.Join(", ", activeBorrowsList.Select(bt => bt.TransactionId))}");
            Console.WriteLine($"[UpdateAsync] New TotalCopies: {dto.TotalCopies}");

            if (dto.TotalCopies < 0)
            {
                Console.WriteLine("[UpdateAsync] Validation Failed: Total copies cannot be negative.");
                return (false, "Total copies cannot be negative.");
            }

            if (dto.TotalCopies < activeBorrows)
            {
                Console.WriteLine($"[UpdateAsync] Validation Failed: Total copies ({dto.TotalCopies}) cannot be less than active borrows ({activeBorrows}).");
                return (false, $"Total copies ({dto.TotalCopies}) cannot be less than active borrows ({activeBorrows}).");
            }

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.ISBN = dto.ISBN;
            book.Description = dto.Description;
            book.GenreId = dto.GenreId;
            book.TotalCopies = dto.TotalCopies;
            book.AvailableCopies = dto.TotalCopies - activeBorrows;
            if (dto.CoverImage != null)
                book.CoverImageUrl = await SaveImageAsync(dto.CoverImage);

            Console.WriteLine($"[UpdateAsync] After Update - TotalCopies: {book.TotalCopies}, AvailableCopies: {book.AvailableCopies}");

            await _repo.UpdateAsync(book);
            return (true, string.Empty);
        }

        public async Task DeleteAsync(int id)
        {
            var activeBorrows = await _context.BorrowTransactions
                .CountAsync(bt => bt.BookId == id && bt.ReturnDate == null);
            if (activeBorrows > 0)
            {
                Console.WriteLine($"[DeleteAsync] Error: Cannot delete book {id} with {activeBorrows} active borrows.");
                throw new InvalidOperationException("Cannot delete a book with active borrows.");
            }

            await _repo.DeleteAsync(id);
        }
    }
}