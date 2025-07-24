namespace library_management_system_backend.Application.Interfaces.Whishlists
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::library_management_system_backend.Application.DTOs.Wishlists;

    namespace library_management_system_backend.Application.Interfaces
    {
        public interface IWishlistService
        {
            Task<IEnumerable<WishlistDto>> GetAllAsync();
            Task<IEnumerable<WishlistDto>> GetByUserIdAsync(int userId);
            Task AddWishlistItemAsync(int userId, CreateWishlistDto dto);
            Task RemoveWishlistItemAsync(int wishlistId, int userId);
        }
    }
}
