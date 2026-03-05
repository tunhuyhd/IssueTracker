using IssueTracker.Application.Common.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IssueTracker.WebApi.Attributes;

/// <summary>
/// Authorization filter that validates user permissions
/// </summary>
public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
	private readonly string[] _permissionCodes;
	private readonly bool _requireAll;
	private readonly ICurrentUser _currentUser;

	public PermissionAuthorizationFilter(
		string[] permissionCodes, 
		bool requireAll,
		ICurrentUser currentUser)
	{
		_permissionCodes = permissionCodes ?? Array.Empty<string>();
		_requireAll = requireAll;
		_currentUser = currentUser;
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

		// If no permissions specified, just require authentication
		if (_permissionCodes.Length == 0)
		{
			return;
		}

		// Get user's permissions
		var userPermissions = _currentUser.GetPermissions();

		// Check permissions
		bool hasPermission = _requireAll
			? _permissionCodes.All(p => userPermissions.Contains(p))
			: _permissionCodes.Any(p => userPermissions.Contains(p));

		if (!hasPermission)
		{
			var requiredPermissions = string.Join(", ", _permissionCodes);
			var requirement = _requireAll ? "all" : "at least one";
			
			context.Result = new ObjectResult(new
			{
				error = "Forbidden",
				message = $"User does not have required permission(s). Required {requirement} of: {requiredPermissions}",
				requiredPermissions = _permissionCodes,
				requireAll = _requireAll
			})
			{
				StatusCode = StatusCodes.Status403Forbidden
			};
		}

		await Task.CompletedTask;
	}
}
