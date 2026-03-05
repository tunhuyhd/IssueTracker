using IssueTracker.Application.Common.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IssueTracker.WebApi.Attributes;

/// <summary>
/// Authorization filter that validates user roles
/// </summary>
public class RoleAuthorizationFilter : IAsyncAuthorizationFilter
{
	private readonly string[] _roleCodes;
	private readonly ICurrentUser _currentUser;

	public RoleAuthorizationFilter(
		string[] roleCodes,
		ICurrentUser currentUser)
	{
		_roleCodes = roleCodes ?? Array.Empty<string>();
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

		// If no roles specified, just require authentication
		if (_roleCodes.Length == 0)
		{
			return;
		}

		// Get user's role
		var userRole = _currentUser.GetRoleCode();

		// Check if user has any of the required roles
		bool hasRole = _roleCodes.Contains(userRole);

		if (!hasRole)
		{
			var requiredRoles = string.Join(", ", _roleCodes);

			context.Result = new ObjectResult(new
			{
				error = "Forbidden",
				message = $"User does not have required role(s). Required one of: {requiredRoles}",
				requiredRoles = _roleCodes,
				userRole = userRole
			})
			{
				StatusCode = StatusCodes.Status403Forbidden
			};
		}

		await Task.CompletedTask;
	}
}
