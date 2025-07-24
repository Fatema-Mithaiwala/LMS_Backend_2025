using library_management_system_backend.Application.DTOs.Genres;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library_management_system_backend.Application.Interfaces.Genres
{
    public interface IGenreService
    {
        Task<IEnumerable<GenreDto>> GetAllAsync();
    }
}