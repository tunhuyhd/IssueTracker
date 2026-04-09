using IssueTracker.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace IssueTracker.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // Read SMTP settings from configuration
        _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
        _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
        _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "";
        _fromName = _configuration["EmailSettings:FromName"] ?? "IssueTracker System";
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        try
        {
            // If SMTP is not configured, log and return true (for development purposes)
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                _logger.LogWarning("SMTP credentials not configured. Email would be sent to: {ToEmail}, Subject: {Subject}", toEmail, subject);
                _logger.LogInformation("Email Body: {Body}", body);
                return true;
            }

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendProjectInvitationEmailAsync(string recipientEmail, string senderEmail, string projectName, string roleName)
    {
        var appUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
        var subject = "Lời mời tham gia dự án - IssueTracker";

        var body = $@"Xin chào,

Từ hệ thống IssueTracker: {senderEmail} vừa mời bạn tham gia dự án ""{projectName}"" với vai trò ""{roleName}"".

--- HƯỚNG DẪN ---

● Nếu bạn ĐÃ CÓ TÀI KHOẢN:
  1. Đăng nhập vào hệ thống IssueTracker tại: {appUrl}/login
  2. Vào mục ""Lời mời"" (Invitations) để xem và chấp nhận lời mời

● Nếu bạn CHƯA CÓ TÀI KHOẢN:
  1. Đăng ký tài khoản tại: {appUrl}/register
  2. QUAN TRỌNG: Sử dụng đúng email này ({recipientEmail}) khi đăng ký
  3. Sau khi đăng ký và đăng nhập, vào mục ""Lời mời"" để chấp nhận

Lưu ý: Lời mời này chỉ có hiệu lực khi bạn sử dụng đúng email {recipientEmail}

---

Nếu bạn không mong muốn nhận lời mời này, bạn có thể bỏ qua email này.

Trân trọng,
IssueTracker System
";

        return await SendEmailAsync(recipientEmail, subject, body, false);
    }
}
