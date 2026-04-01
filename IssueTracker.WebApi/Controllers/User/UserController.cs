using IssueTracker.Application.Common.Services;
using IssueTracker.Application.Users.Commands;
using Microsoft.AspNetCore.Http;
using IssueTracker.Application.Common.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IssueTracker.WebApi.Attributes;
using IssueTracker.Application.Common.Interfaces;

namespace IssueTracker.WebApi.Controllers.User;

[Authorize]
public class UserController : BaseApiController
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public UserController(ICurrentUserService currentUserService, IFileStorageService fileStorageService)
    {
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    [HttpPut("update-me")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfile([FromForm] IssueTracker.WebApi.Models.UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProfileCommand
        {
            FullName = request.FullName,
            Image = request.Image
        };

        var result = await Mediator.Send(command, cancellationToken);

        // Set refresh token cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", result.Token.RefreshToken ?? string.Empty, cookieOptions);

        // Ensure returned user image URLs are absolute for local storage
        var returnedUser = result.User;
        var absImageUrl = returnedUser.ImageUrl != null ? _fileStorageService.GetFileUri(returnedUser.ImageUrl) : null;
        var absThumb = returnedUser.ImageUrl != null ? _fileStorageService.GetThumbnailUrl(returnedUser.ImageUrl, 150, 150) : null;
        var absSmall = returnedUser.ImageUrl != null ? _fileStorageService.GetThumbnailUrl(returnedUser.ImageUrl, 50, 50) : null;

        return Ok(new
        {
            accessToken = result.Token.AccessToken,
            expiresIn = result.Token.ExpiresIn,
            user = new
            {
                id = returnedUser.Id,
                username = returnedUser.Username,
                email = returnedUser.Email,
                fullName = returnedUser.FullName,
                imageUrl = absImageUrl ?? returnedUser.ImageUrl,
                imageUrlThumbnail = absThumb ?? returnedUser.ImageUrlThumbnail,
                imageUrlSmall = absSmall ?? returnedUser.ImageUrlSmall,
                isActive = returnedUser.IsActive
            }
        });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var user = await _currentUserService.GetCurrentUserWithRoleAsync(cancellationToken);
        if (user == null)
            return Unauthorized(new { message = "User not found" });

        var absImage = user.ImageUrl != null ? _fileStorageService.GetFileUri(user.ImageUrl) : null;

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            fullName = user.FullName,
            imageUrl = absImage ?? user.ImageUrl,
            imageUrlThumbnail = _fileStorageService.GetThumbnailUrl(user.ImageUrl, 150, 150),
            imageUrlSmall = _fileStorageService.GetThumbnailUrl(user.ImageUrl, 50, 50),
            isActive = user.IsActive,
            role = new
            {
                code = user.Role?.Code,
                description = user.Role?.Description
            },
            permissions = user.Role?.Permissions?.Select(p => p.Code).ToArray() ?? Array.Empty<string>()
        });
    }

    [HttpGet("me/projects")]
    public async Task<IActionResult> GetMyProjects(CancellationToken cancellationToken)
    {
        var projects = await _currentUserService.GetUserProjectsAsync(cancellationToken);

        return Ok(projects.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            description = p.Description,
            isEnabled = p.IsEnabled,
            createdOn = p.CreatedOn
        }));
    }

    [HttpGet("projects/{projectId}/role")]
    public async Task<IActionResult> GetMyProjectRole(Guid projectId, CancellationToken cancellationToken)
    {
        var isMember = await _currentUserService.IsMemberOfProjectAsync(projectId, cancellationToken);
        if (!isMember)
            return NotFound(new { message = "You are not a member of this project" });

        var projectRole = await _currentUserService.GetProjectRoleAsync(projectId, cancellationToken);

        return Ok(new
        {
            projectId,
            role = new
            {
                id = projectRole?.Id,
                code = projectRole?.Code,
                description = projectRole?.Description
            },
            isOwner = await _currentUserService.IsProjectOwnerAsync(projectId, cancellationToken)
        });
    }
}
