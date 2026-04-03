using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Dto.Auth;
using IssueTracker.Application.Common.Interfaces;
using IssueTracker.Application.Identity;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Application.Users.Commands;

public class GoogleAuthCommand : IRequest<AuthResponse>
{
    public string IdToken { get; set; } = string.Empty;
}

public class GoogleAuthCommandHandler(
	IGoogleAuthService googleAuthService,
	IRepository<User> userRepository,
	IApplicationDbContext dbContext,
	ITokenService tokenService,
	IFileStorageService fileStorageService,
	ILogger<GoogleAuthCommandHandler> logger
) : IRequestHandler<GoogleAuthCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(GoogleAuthCommand request, CancellationToken cancellationToken)
    {
        // Verify Google token
        var googleUser = await googleAuthService.VerifyGoogleTokenAsync(request.IdToken, cancellationToken);

        if (!googleUser.EmailVerified)
        {
            throw new InvalidOperationException("Email not verified by Google");
        }

        // Check if user exists with this Google account
        var existingUser = await dbContext.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(
                u => u.ExternalLoginProvider == "Google" && u.ExternalLoginProviderKey == googleUser.Sub,
                cancellationToken);

        User user;

        if (existingUser == null)
        {
            // Check if user exists with this email
            var userWithEmail = await dbContext.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.Permissions)
                .FirstOrDefaultAsync(u => u.Email == googleUser.Email, cancellationToken);

            if (userWithEmail != null)
            {
                // Link existing account with Google
                userWithEmail.ExternalLoginProvider = "Google";
                userWithEmail.ExternalLoginProviderKey = googleUser.Sub;

                if (string.IsNullOrWhiteSpace(userWithEmail.ImageUrl) && !string.IsNullOrWhiteSpace(googleUser.Picture))
                {
                    userWithEmail.ImageUrl = googleUser.Picture;
                }

                await userRepository.UpdateAsync(userWithEmail, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                user = userWithEmail;

                logger.LogInformation("Linked existing user {Email} with Google account", googleUser.Email);
            }
            else
            {
                var roleUser = await dbContext.Roles
                    .Include(r => r.Permissions)
                    .FirstOrDefaultAsync(r => r.Code == "USER", cancellationToken);

                if (roleUser == null)
                {
                    throw new InvalidOperationException("USER role not found. Please seed the database.");
                }

                // Generate unique username from email
                var username = await GenerateUniqueUsernameAsync(googleUser.Email, cancellationToken);

                user = new User
                {
                    Username = username,
                    Email = googleUser.Email,
                    FullName = googleUser.Name,
                    ImageUrl = googleUser.Picture,
                    IsActive = true,
                    RoleId = roleUser.Id,
                    Role = roleUser,
                    ExternalLoginProvider = "Google",
                    ExternalLoginProviderKey = googleUser.Sub,
                    PasswordHash = string.Empty, // No password for Google users
                    Salt = null
                };

                await userRepository.AddAsync(user, cancellationToken);
                // IMPORTANT: Save changes to commit new user to database
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Created new user from Google: {Email}", googleUser.Email);
            }
        }
        else
        {
            user = existingUser;
            logger.LogInformation("User logged in with Google: {Email}", googleUser.Email);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new InvalidOperationException("User account is disabled");
        }

        // Generate tokens using User object (không cần query lại DB)
        var tokenResponse = await tokenService.GenerateTokenAsync(user, cancellationToken);

        // Refresh token đã được set trong GenerateTokenAsync
        await dbContext.SaveChangesAsync(cancellationToken);

        // Get absolute image URLs
        var absImageUrl = user.ImageUrl != null && !user.ImageUrl.StartsWith("http")
            ? fileStorageService.GetFileUri(user.ImageUrl)
            : user.ImageUrl;

        var absThumb = user.ImageUrl != null && !user.ImageUrl.StartsWith("http")
            ? fileStorageService.GetThumbnailUrl(user.ImageUrl, 150, 150)
            : null;

        var absSmall = user.ImageUrl != null && !user.ImageUrl.StartsWith("http")
            ? fileStorageService.GetThumbnailUrl(user.ImageUrl, 50, 50)
            : null;

        return new AuthResponse
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken ?? string.Empty,
            ExpiresIn = tokenResponse.ExpiresIn,
            User = new AuthUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                ImageUrl = absImageUrl,
                ImageUrlThumbnail = absThumb,
                ImageUrlSmall = absSmall,
                IsActive = user.IsActive,
                Role = user.Role != null ? new Common.Dto.Roles.RoleDto
                {
                    Id = user.Role.Id,
                    Code = user.Role.Code ?? string.Empty,
                    Description = user.Role.Description ?? string.Empty,
                    Permissions = user.Role.Permissions?.Select(p => new Common.Dto.Roles.PermissionDto
                    {
                        Id = p.Id,
                        Code = p.Code,
                    }).ToList() ?? new List<Common.Dto.Roles.PermissionDto>()
                } : null,
                Permissions = user.Role?.Permissions?.Select(p => p.Code).ToArray() ?? Array.Empty<string>()
            }
        };
    }

    private async Task<string> GenerateUniqueUsernameAsync(string email, CancellationToken cancellationToken)
    {
        var baseUsername = email.Split('@')[0].ToLowerInvariant();
        var username = baseUsername;
        var counter = 1;

        while (await dbContext.Users.AnyAsync(u => u.Username == username, cancellationToken))
        {
            username = $"{baseUsername}{counter}";
            counter++;
        }

        return username;
    }
}
