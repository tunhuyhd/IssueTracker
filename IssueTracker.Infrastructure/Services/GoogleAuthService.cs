using Google.Apis.Auth;
using IssueTracker.Application.Common.Dto.Auth;
using IssueTracker.Application.Common.Interfaces;
using IssueTracker.Infrastructure.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IssueTracker.Infrastructure.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleSettings _googleSettings;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        IOptions<GoogleSettings> googleSettings,
        ILogger<GoogleAuthService> logger)
    {
        _googleSettings = googleSettings.Value;
        _logger = logger;
    }

    public async Task<GoogleUserInfo> VerifyGoogleTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleSettings.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (payload == null)
            {
                throw new InvalidOperationException("Invalid Google token");
            }

            return new GoogleUserInfo
            {
                Sub = payload.Subject,
                Email = payload.Email,
                EmailVerified = payload.EmailVerified,
                Name = payload.Name,
                GivenName = payload.GivenName,
                FamilyName = payload.FamilyName,
                Picture = payload.Picture
            };
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogError(ex, "Invalid Google JWT token");
            throw new InvalidOperationException("Invalid Google token", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Google token");
            throw;
        }
    }
}
