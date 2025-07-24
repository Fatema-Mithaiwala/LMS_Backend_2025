namespace library_management_system_backend.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string otp);
        Task SendBookAvailabilityEmailAsync(string toEmail, string bookTitle);
        Task SendLibrarianWelcomeEmailAsync(string toEmail, string fullName, string username, string temporaryPassword);
        Task SendBorrowRequestRejectedEmailAsync(string toEmail, string bookTitle, string? remarks);
        Task SendBorrowRequestApprovedEmailAsync(string toEmail, string bookTitle, DateTime dueDate);
        Task SendReturnRequestApprovedEmailAsync(string toEmail, string bookTitle);
        Task SendReturnRequestRejectedEmailAsync(string toEmail, string bookTitle, string? remarks);

        }
}
