using IssueTracker.Application.Common.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IssueTracker.WebApi.Attributes;

/// <summary>
/// Authorization filter that validates user is a member of a project
/// </summary>
public class ProjectMemberAuthorizationFilter : IAsyncAuthorizationFilter
{
	private readonly string _projectIdParameterName;
	private readonly ICurrentUser _currentUser;
	private readonly IProjectAuthorizationService _projectAuthorizationService;

	public ProjectMemberAuthorizationFilter(
		string projectIdParameterName,
		ICurrentUser currentUser,
		IProjectAuthorizationService projectAuthorizationService)
	{
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
		}
	}
}
