using library_management_system_backend.Application.DTOs.Genres;
using library_management_system_backend.Application.Interfaces.Genres;
using library_management_system_backend.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace library_management_system_backend.Application.Services
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _repo;

        public GenreService(IGenreRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<GenreDto>> GetAllAsync()
        {
            var genres = await _repo.GetAllAsync();
            return genres.Select(g => new GenreDto
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName
            });
        }
    }
}