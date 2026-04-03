using IssueTracker.Application.Common.Dto.Auth;

namespace IssueTracker.Application.Common.Interfaces;

/// Service xử lý Google OAuth2 authentication
public interface IGoogleAuthService
{
    /// Verify Google ID Token và lấy thông tin user
    Task<GoogleUserInfo> VerifyGoogleTokenAsync(string idToken, CancellationToken cancellationToken = default);
}
