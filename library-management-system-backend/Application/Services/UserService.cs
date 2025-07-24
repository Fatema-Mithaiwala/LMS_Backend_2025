using library_management_system_backend.Application.DTOs;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Domain.Enums;
using library_management_system_backend.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace library_management_system_backend.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IAuthRepository _authRepo;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepo, IAuthRepository authRepo, IEmailService emailService)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _authRepo = authRepo ?? throw new ArgumentNullException(nameof(authRepo));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                RoleName = u.Role?.RoleName ?? "Unknown",
                IsBlocked = u.IsBlocked
            });
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null || user.IsDeleted)
                throw new ArgumentException("User not found");

            return new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleName = user.Role?.RoleName ?? "Unknown",
                IsBlocked = user.IsBlocked
            };
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(RoleEnum role)
        {
            var users = await _userRepo.GetUsersByRoleAsync((int)role);
            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                RoleName = u.Role?.RoleName ?? "Unknown",
                IsBlocked = u.IsBlocked
            });
        }

        public async Task SoftDeleteUserAsync(int userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null || user.IsDeleted)
                throw new ArgumentException("User not found");

            user.IsDeleted = true;
            await _userRepo.UpdateUserAsync(user);
        }

        public async Task UpdateUserAsync(int userId, UpdateUserDto dto)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null || user.IsDeleted)
                throw new ArgumentException("User not found");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;

            await _userRepo.UpdateUserAsync(user);
        }

        public async Task<object> CreateUserAsync(RegisterDto dto, int roleId)
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new ArgumentException("Passwords do not match");

            var existingUser = await _authRepo.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("User already exists");

            var validRole = await _authRepo.IsValidRoleId(roleId);
            if (!validRole)
                throw new ArgumentException("Invalid role ID");

            var newUser = new Domain.Entities.User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = PasswordHelper.Hash(dto.Password),
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _authRepo.AddUserAsync(newUser);

            var role = await _authRepo.GetRoleByIdAsync(roleId);
            if (role?.RoleName == "Librarian")
            {
                try
                {
                    await _emailService.SendLibrarianWelcomeEmailAsync(
                        newUser.Email,
                        newUser.FullName,
                        newUser.Email, 
                        dto.Password
                    );
                    Console.WriteLine($"Welcome email sent to librarian {newUser.Email}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send welcome email to {newUser.Email}: {ex.Message}");
                }
            }

            return new { message = "User created successfully", roleId };
        }

        public async Task ToggleBlockUserAsync(int userId, bool block)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null || user.IsDeleted)
                throw new ArgumentException("User not found");

            user.IsBlocked = block;
            await _userRepo.UpdateUserAsync(user);
        }
    }
}