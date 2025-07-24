using library_management_system_backend.Application.DTOs;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Domain.Entities;
using library_management_system_backend.Domain.Helpers;

namespace library_management_system_backend.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private static readonly Dictionary<string, string> _otpStore = new();
        private readonly IUserRepository _userRepo;


        public AuthService(IAuthRepository authRepo, IConfiguration configuration, IEmailService emailService, IUserRepository userRepo
)
        {
            _userRepo = userRepo;
            _authRepo = authRepo;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<object> RegisterAsync(RegisterDto dto, int roleId)
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new ArgumentException("Passwords do not match");

            var existingUser = await _authRepo.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("User already exists");

            var validRole = await _authRepo.IsValidRoleId(roleId);
            if (!validRole)
                throw new ArgumentException("Invalid role ID");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = PasswordHelper.Hash(dto.Password),
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _authRepo.AddUserAsync(user);
            return new { message = "Registration successful", roleId };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _authRepo.GetUserByEmailAsync(dto.Email);

            if (user == null || user.IsDeleted || user.IsBlocked)
                throw new UnauthorizedAccessException("Account is inactive or blocked.");

            if (!PasswordHelper.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password");

            return new LoginResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.RoleName ?? "Student",
                Phone = user.PhoneNumber,
                Token = JwtHelper.GenerateToken(user, _configuration)
            };
        }

        public async Task GenerateAndSendOtpAsync(string email)
        {
            var user = await _authRepo.GetUserByEmailAsync(email);
            if (user == null || user.IsDeleted)
                throw new ArgumentException("User not found");

            var otp = new Random().Next(100000, 999999).ToString();
            _otpStore[email] = otp;

            await _emailService.SendPasswordResetEmailAsync(email, otp);
        }

        public async Task ResetPasswordAsync(string email, string otp, string newPassword)
        {
            if (!_otpStore.TryGetValue(email, out var storedOtp) || storedOtp != otp)
                throw new UnauthorizedAccessException("Invalid OTP");

            var user = await _authRepo.GetUserByEmailAsync(email);
            if (user == null || user.IsDeleted)
                throw new ArgumentException("User not found");

            user.PasswordHash = PasswordHelper.Hash(newPassword);
            await _userRepo.UpdateUserAsync(user);
            _otpStore.Remove(email);
        }
    }
}
