namespace IssueTracker.Application.Common.Dto.Auth;

/// Request model cho Google OAuth2 authentication
public class GoogleAuthRequest
{
    /// Google ID Token nhận từ Google Sign-In
    public string IdToken { get; set; } = string.Empty;
}
