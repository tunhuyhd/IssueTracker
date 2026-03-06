using IssueTracker.Application.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Controllers.User;

[Authorize]
public class UserController : BaseApiController
{
    private readonly ICurrentUserService _currentUserService;

    public UserController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var user = await _currentUserService.GetCurrentUserWithRoleAsync(cancellationToken);
        if (user == null)
            return Unauthorized(new { message = "User not found" });

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            fullName = user.FullName,
            imageUrl = user.ImageUrl,
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
