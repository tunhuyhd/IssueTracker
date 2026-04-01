using IssueTracker.Application.Common.Dto;
using IssueTracker.Application.Identity;
using IssueTracker.Application.Common.Interfaces;
using IssueTracker.Application.Common.Services;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;

namespace IssueTracker.Application.Users.Commands;

public class UpdateProfileCommand : IRequest<UpdateProfileResult>
{
    public string? FullName { get; set; }
    public IFormFile? Image { get; set; }
}

public class UpdateProfileResult
{
    public TokenResponse Token { get; set; } = default!;
    public UserProfileDto User { get; set; } = default!;
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageUrlThumbnail { get; set; }
    public string? ImageUrlSmall { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateProfileCommandHandler(
    IRepository<User> userRepository,
    IFileStorageService fileStorageService,
    ITokenService tokenService,
    ICurrentUserService currentUserService,
    ILogger<UpdateProfileCommandHandler> logger)
    : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
{
    public async Task<UpdateProfileResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();
        if (userId == Guid.Empty)
        {
            logger.LogWarning("UpdateProfile attempted by unauthenticated user");
            throw new InvalidOperationException("User not authenticated");
        }

        var user = await userRepository.GetByIdAsync(userId, null, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName;
        }

        // Handle image upload if provided
        if (request.Image != null && request.Image.Length > 0)
        {
            try
            {
                // Validate image file
                if (!IsValidImageFile(request.Image))
                {
                    throw new InvalidOperationException("Invalid image file. Please upload a valid image (.jpg, .jpeg, .png, .gif, .webp)");
                }

                // Delete old image if exists
                if (!string.IsNullOrWhiteSpace(user.ImageUrl))
                {
                    var deleteResult = await fileStorageService.DeleteFileAsync(user.ImageUrl);
                    if (!deleteResult)
                    {
                        logger.LogWarning("Failed to delete old image for user {UserId}: {ImageUrl}", userId, user.ImageUrl);
                    }
                }

                // Upload new image
                var fileUploadRequest = new FileUploadRequest { File = request.Image };
                var subFolder = $"users/{userId}";
                var allowedFileTypes = new[] { FileType.Image };

                var imageUrl = await fileStorageService.UploadFileAsync(
                    fileUploadRequest,
                    allowedFileTypes,
                    subFolder,
                    false,
                    cancellationToken);

                user.ImageUrl = imageUrl;

                // For backward compatibility, extract public ID for Cloudinary URLs
                if (imageUrl.Contains("cloudinary.com"))
                {
                    user.ImagePublicId = ExtractPublicIdFromCloudinaryUrl(imageUrl);
                }
                else
                {
                    // For local storage, use relative path as public ID
                    user.ImagePublicId = imageUrl;
                }

                logger.LogInformation("Profile image updated successfully for user {UserId}. New URL: {ImageUrl}", userId, imageUrl);
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                // Re-throw validation exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to upload image for user {UserId}", userId);
                throw new InvalidOperationException("Failed to process image upload. Please try again or contact support.", ex);
            }
        }

        await userRepository.UpdateAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        // Generate new tokens (also updates refresh token in DB)
        var token = await tokenService.GenerateTokenAsync(user.Id, cancellationToken);

        // Create user profile DTO with thumbnail URLs
        var userProfileDto = new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            ImageUrl = user.ImageUrl,
            ImageUrlThumbnail = fileStorageService.GetThumbnailUrl(user.ImageUrl, 150, 150),
            ImageUrlSmall = fileStorageService.GetThumbnailUrl(user.ImageUrl, 50, 50),
            IsActive = user.IsActive
        };

        return new UpdateProfileResult
        {
            Token = token,
            User = userProfileDto
        };
    }

    private static bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;

        // Check file extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            return false;

        // Check MIME type
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (string.IsNullOrEmpty(file.ContentType) || !allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            return false;

        // Check file size (max 10MB)
        if (file.Length > 10 * 1024 * 1024)
            return false;

        return true;
    }

    private string ExtractPublicIdFromCloudinaryUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');

            var uploadIndex = Array.IndexOf(segments, "upload");
            if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length) return string.Empty;

            var pathAfterVersion = segments[(uploadIndex + 2)..];
            var publicIdWithExtension = string.Join("/", pathAfterVersion);

            return Path.GetFileNameWithoutExtension(publicIdWithExtension);
        }
        catch
        {
            return string.Empty;
        }
    }
}
