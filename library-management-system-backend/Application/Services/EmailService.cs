using library_management_system_backend.Application.Interfaces;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace library_management_system_backend.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _fromEmail;
        private readonly string _password;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException("SmtpServer configuration is missing.");
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? throw new ArgumentNullException("SmtpPort configuration is missing."));
            _fromEmail = configuration["EmailSettings:Email"] ?? throw new ArgumentNullException("Email configuration is missing.");
            _password = configuration["EmailSettings:Password"] ?? throw new ArgumentNullException("Password configuration is missing.");
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string otp)
        {
            var fromAddress = new MailAddress(_fromEmail, "Pixel Page Team");
            var toAddress = new MailAddress(toEmail);
            const string subject = "Password Reset Request";
            string body = $@"
            <p>Dear User,</p>
            <p>We received a request to reset your password. Please use the following OTP to proceed with the password reset:</p>
            <p><strong>Your OTP: {otp}</strong></p>
            <p>To reset your password, enter this OTP on the password reset page within your application and follow the instructions there to set a new password.</p>
            <p>If you did not request a password reset, please ignore this email.</p>
            <p>Best regards,<br/>Pixel Page Team</p>
            ";

            await SendEmailAsync(fromAddress, toAddress, subject, body);
        }

        public async Task SendLibrarianWelcomeEmailAsync(string toEmail, string fullName, string username, string temporaryPassword)
        {
            var fromAddress = new MailAddress(_fromEmail, "Pixel Page Team");
            var toAddress = new MailAddress(toEmail);
            const string subject = "Welcome to Pixel Page Library System!";
            string body = $@"
               <p>Dear {fullName},</p>
               <p>Welcome to the Pixel Page Library Management System! Your librarian account has been created successfully.</p>
               <p><strong>Your Login Details:</strong></p>
               <ul>
                   <li><strong>Username:</strong> {username}</li>
                   <li><strong>Temporary Password:</strong> {temporaryPassword}</li>
               </ul>
               <p>Please log in to the system and change your password immediately for security purposes.</p>
               <p>If you have any questions, contact the admin team.</p>
               <p>Best regards,<br/>Pixel Page Team</p>
               ";

            await SendEmailAsync(fromAddress, toAddress, subject, body);
        }

        public async Task SendBookAvailabilityEmailAsync(string toEmail, string bookTitle)
        {
            var fromAddress = new MailAddress(_fromEmail, "Pixel Page Team");
            var toAddress = new MailAddress(toEmail);
            const string subject = "Book Now Available!";
            string body = $@"
            <p>Dear User,</p>
            <p>Great news! The book <strong>{bookTitle}</strong> you added to your wishlist is now available in the Pixel Page Library.</p>
            <p>Please visit the library application to borrow the book before it’s reserved by someone else.</p>
            <p>If you no longer wish to borrow this book, you can remove it from your wishlist in the application.</p>
            <p>Happy reading!<br/>Pixel Page Team</p>
            ";

            await SendEmailAsync(fromAddress, toAddress, subject, body);
        }

        public async Task SendBorrowRequestApprovedEmailAsync(string toEmail, string bookTitle, DateTime dueDate)
        {
            var fromAddress = new MailAddress(_fromEmail, "Pixel Page Team");
            var toAddress = new MailAddress(toEmail);
            const string subject = "Borrow Request Approved!";
            string body = $@"
            <p>Dear User,</p>
            <p>We are pleased to inform you that your borrow request for the book <strong>{bookTitle}</strong> has been approved.</p>
            <p><strong>Due Date:</strong> {dueDate:yyyy-MM-dd}</p>
            <p>Please visit the library to pick up your book before the due date. Enjoy your reading!</p>
            <p>Best regards,<br/>Pixel Page Team</p>
            ";

            await SendEmailAsync(fromAddress, toAddress, subject, body);
        }

        public async Task SendBorrowRequestRejectedEmailAsync(string toEmail, string bookTitle, string? remarks)
        {
            var fromAddress = new MailAddress(_fromEmail, "Pixel Page Team");
            var toAddress = new MailAddress(toEmail);
            const string subject = "Borrow Request Rejected";
            string body = $@"
            <p>Dear User,</p>
            <p>We regret to inform you that your borrow request for the book <strong>{bookTitle}</strong> has been rejected.</p>
            {(remarks != null ? $"<p><strong>Reason:</strong> {remarks}</p>" : "")}
            <p>Please contact the librarian if you have any questions or try requesting another book.</p>
            <p>Best regards,<br/>Pixel Page Team</p>
            ";

            await SendEmailAsync(fromAddress, toAddress, subject, body);
        }

        public async Task SendReturnRequestApprovedEmailAsync(string toEmail, string bookTitle)
        {
            var fromAddress = new MailAddress(_fromEmail, "Pixel Page Team");
            var toAddress = new MailAddress(toEmail);
            const string subject = "Return Request Approved!";
            string body = $@"
            <p>Dear User,</p>
            <p>We are pleased to inform you that your return request for the book <strong>{bookTitle}</strong> has been approved.</p>
            <p>The book has been successfully returned to the library. Thank you for using our services!</p>
            <p>Best regards,<br/>Pixel Page Team</p>
            ";

            await SendEmailAsync(fromAddress, toAddress, subject, body);
        }

        public async Task SendReturnRequestRejectedEmailAsync(string toEmail, string bookTitle, string? remarks)
        {
            var fromAddress = new MailAddress(_fromEmail, "Pixel Page Team");
            var toAddress = new MailAddress(toEmail);
            const string subject = "Return Request Rejected";
            string body = $@"
            <p>Dear User,</p>
            <p>We regret to inform you that your return request for the book <strong>{bookTitle}</strong> has been rejected.</p>
            {(remarks != null ? $"<p><strong>Reason:</strong> {remarks}</p>" : "")}
            <p>Please contact the librarian for further details or to resolve any issues.</p>
            <p>Best regards,<br/>Pixel Page Team</p>
            ";

            await SendEmailAsync(fromAddress, toAddress, subject, body);
        }

        private async Task SendEmailAsync(MailAddress fromAddress, MailAddress toAddress, string subject, string body)
        {
            using (var message = new MailMessage())
            {
                message.From = fromAddress;
                message.To.Add(toAddress);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(_fromEmail, _password);
                    smtpClient.EnableSsl = true;
                    try
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                    catch (SmtpException ex)
                    {
                        Console.WriteLine($"Failed to send email to {toAddress.Address}: {ex.Message}");
                    }
                }
            }
        }
    }
}