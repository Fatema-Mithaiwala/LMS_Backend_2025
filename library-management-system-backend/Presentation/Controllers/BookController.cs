using library_management_system_backend.Application.DTOs.Books;
using library_management_system_backend.Application.Interfaces.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace library_management_system_backend.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var books = await _bookService.GetAllAsync();
                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving books: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var book = await _bookService.GetByIdAsync(id);
                if (book == null) return NotFound("Book not found");
                return Ok(book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the book: " + ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateBookDto dto)
        {
            await _bookService.CreateAsync(dto);
            return Ok(new { message = "Book created successfully" });
        }

        [Authorize(Roles = "Admin,Librarian")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateBookDto dto)
        {
            Console.WriteLine($"[BookController.Update] Updating BookId: {id}, TotalCopies: {dto.TotalCopies}");
            var (success, errorMessage) = await _bookService.UpdateAsync(id, dto);
            if (!success)
            {
                Console.WriteLine($"[BookController.Update] Update Failed: {errorMessage}");
                return StatusCode(500, new { message = "Failed to update book.", error = errorMessage });
            }
            return Ok(new { message = "Book updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _bookService.DeleteAsync(id);
            return Ok(new { message = "Book deleted successfully" });
        }
    }

}
