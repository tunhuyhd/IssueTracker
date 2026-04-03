namespace IssueTracker.Infrastructure.Auth;

/// Google OAuth2 configuration settings
public class GoogleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
