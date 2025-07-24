using library_management_system_backend.Application.DTOs.Wishlists;
using library_management_system_backend.Application.Exceptions;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Application.Interfaces.Books;
using library_management_system_backend.Application.Interfaces.Notifications;
using library_management_system_backend.Application.Interfaces.Whishlists.library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace library_management_system_backend.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly INotificationService _notificationService;

        public WishlistService(
            IWishlistRepository wishlistRepository,
            IUserRepository userRepository,
            IBookRepository bookRepository,
            INotificationService notificationService)
        {
            _wishlistRepository = wishlistRepository ?? throw new ArgumentNullException(nameof(wishlistRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<IEnumerable<WishlistDto>> GetAllAsync()
        {
            var wishlists = await _wishlistRepository.GetAllAsync();
            return wishlists.Select(w => MapToDto(w));
        }

        public async Task<IEnumerable<WishlistDto>> GetByUserIdAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            var wishlists = await _wishlistRepository.GetByUserIdAsync(userId);
            return wishlists.Select(w => MapToDto(w));
        }

        public async Task AddWishlistItemAsync(int userId, CreateWishlistDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            var book = await _bookRepository.GetByIdAsync(dto.BookId)
                ?? throw new NotFoundException($"Book with ID {dto.BookId} not found.");

            if (await _wishlistRepository.ExistsAsync(userId, dto.BookId))
                throw new ConflictException($"Book with ID {dto.BookId} is already in the wishList for user {userId}.");

            var wishlist = new Wishlist
            {
                UserId = userId,
                BookId = dto.BookId,
                IsNotified = false,
                AddedAt = DateTime.UtcNow,
                User = user,
                Book = book
            };

            await _wishlistRepository.AddAsync(wishlist);

            if (book.AvailableCopies > 0)
            {
                await _notificationService.CreateBookAvailabilityNotificationAsync(dto.BookId, userId);
            }
        }

        public async Task RemoveWishlistItemAsync(int wishlistId, int userId)
        {
            var wishlist = await _wishlistRepository.GetByIdAsync(wishlistId)
                ?? throw new NotFoundException($"Wishlist entry with ID {wishlistId} not found.");

            if (wishlist.UserId != userId)
                throw new UnauthorizedAccessException("Not authorized to delete this wishlist entry.");

            await _wishlistRepository.DeleteAsync(wishlist);
        }

        private static WishlistDto MapToDto(Wishlist wishlist)
        {
            return new WishlistDto
            {
                WishlistId = wishlist.WishlistId,
                UserId = wishlist.UserId,
                BookId = wishlist.BookId,
                BookTitle = wishlist.Book?.Title,
                IsNotified = wishlist.IsNotified,
                CreatedAt = wishlist.AddedAt
            };
        }
    }
}