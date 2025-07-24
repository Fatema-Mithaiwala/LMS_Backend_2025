using library_management_system_backend.Application.DTOs.Genres;
using library_management_system_backend.Application.Interfaces.Genres;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library_management_system_backend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenreController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreDto>>> GetAll()
        {
            var genres = await _genreService.GetAllAsync();
            return Ok(genres);
        }
    }
}