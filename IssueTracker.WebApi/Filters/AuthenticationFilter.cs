using IssueTracker.Application.Common.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IssueTracker.WebApi.Attributes;

/// <summary>
/// Simple authentication filter that only checks if user is authenticated
/// </summary>
public class AuthenticationFilter : IAsyncAuthorizationFilter
{
	private readonly ICurrentUser _currentUser;

	public AuthenticationFilter(ICurrentUser currentUser)
	{
		_currentUser = currentUser;
	}

	public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		if (!_currentUser.IsAuthenticated())
		{
			context.Result = new UnauthorizedObjectResult(new
			{
				error = "Unauthorized",
				message = "User is not authenticated"
			});
		}

		await Task.CompletedTask;
	}
}
