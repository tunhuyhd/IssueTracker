using IssueTracker.Application.Common.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IssueTracker.WebApi.Attributes;

/// <summary>
/// Authorization filter that validates user has required project permissions
/// </summary>
public class ProjectPermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
	private readonly string[] _permissionCodes;
	private readonly bool _requireAll;
	private readonly string _projectIdParameterName;
	private readonly ICurrentUser _currentUser;
	private readonly IProjectAuthorizationService _projectAuthorizationService;

	public ProjectPermissionAuthorizationFilter(
		string[] permissionCodes,
		bool requireAll,
		string projectIdParameterName,
		ICurrentUser currentUser,
		IProjectAuthorizationService projectAuthorizationService)
	{
		_permissionCodes = permissionCodes ?? Array.Empty<string>();
		_requireAll = requireAll;
		_projectIdParameterName = projectIdParameterName;
		_currentUser = currentUser;
		_projectAuthorizationService = projectAuthorizationService;
	}

	public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		// Check if user is authenticated
		if (!_currentUser.IsAuthenticated())
		{
			context.Result = new UnauthorizedObjectResult(new
			{
				error = "Unauthorized",
				message = "User is not authenticated"
			});
			return;
		}

		// Get projectId from route parameters
		if (!context.RouteData.Values.TryGetValue(_projectIdParameterName, out var projectIdValue) ||
			!Guid.TryParse(projectIdValue?.ToString(), out var projectId))
		{
			context.Result = new BadRequestObjectResult(new
			{
				error = "BadRequest",
				message = $"Project ID parameter '{_projectIdParameterName}' is missing or invalid"
			});
			return;
		}

		// Check if user is project member
		if (!await _projectAuthorizationService.IsProjectMemberAsync(projectId))
		{
			context.Result = new ObjectResult(new
			{
				error = "Forbidden",
				message = "User is not a member of this project",
				projectId = projectId
			})
			{
				StatusCode = StatusCodes.Status403Forbidden
			};
			return;
		}

		// If no permissions specified, just require project membership
		if (_permissionCodes.Length == 0)
		{
			return;
		}

		// Check project permissions
		bool hasPermission = _requireAll
			? await _projectAuthorizationService.HasAllProjectPermissionsAsync(projectId, _permissionCodes)
			: await _projectAuthorizationService.HasAnyProjectPermissionAsync(projectId, _permissionCodes);

		if (!hasPermission)
		{
			var requiredPermissions = string.Join(", ", _permissionCodes);
			var requirement = _requireAll ? "all" : "at least one";

			context.Result = new ObjectResult(new
			{
				error = "Forbidden",
				message = $"User does not have required project permission(s). Required {requirement} of: {requiredPermissions}",
				projectId = projectId,
				requiredPermissions = _permissionCodes,
				requireAll = _requireAll
			})
			{
				StatusCode = StatusCodes.Status403Forbidden
			};
		}
	}
}
