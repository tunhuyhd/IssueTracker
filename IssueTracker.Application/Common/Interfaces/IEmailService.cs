namespace IssueTracker.Application.Common.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Send a simple email with subject and body
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body content</param>
    /// <param name="isHtml">Whether the body is HTML formatted</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);

    /// <summary>
    /// Send project invitation email
    /// </summary>
    /// <param name="recipientEmail">Email of the person being invited</param>
    /// <param name="senderEmail">Email of the person sending invitation</param>
    /// <param name="projectName">Name of the project</param>
    /// <param name="roleName">Role/position being offered</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendProjectInvitationEmailAsync(string recipientEmail, string senderEmail, string projectName, string roleName);
}
