using library_management_system_backend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library_management_system_backend.Application.Interfaces.Genres
{
    public interface IGenreRepository
    {
        Task<IEnumerable<Genre>> GetAllAsync();
    }
}