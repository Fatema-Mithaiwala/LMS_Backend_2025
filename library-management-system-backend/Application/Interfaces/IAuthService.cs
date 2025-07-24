using library_management_system_backend.Application.DTOs;

namespace library_management_system_backend.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task<object> RegisterAsync(RegisterDto dto, int roleId);
        Task GenerateAndSendOtpAsync(string email);
        Task ResetPasswordAsync(string email, string otp, string newPassword);
    }
}
