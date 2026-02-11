using IssueTracker.Application.Common.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace IssueTracker.Infrastructure.Auth;

public class CurrentUser : ICurrentUser
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

	public CurrentUser(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public Guid GetUserId() =>
		Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
			? userId
			: Guid.Empty;

	public string GetUsername() => User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

	public string GetUserEmail() => User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

	public string GetFullName() => User?.FindFirstValue("full_name") ?? string.Empty;

	public string GetRoleCode() => User?.FindFirstValue("role_code") ?? string.Empty;

	public string GetImageUrl() => User?.FindFirstValue("image_url") ?? string.Empty;

	public bool IsAuthenticated() => User?.Identity?.IsAuthenticated == true;

	public string[] GetPermissions()
	{
		var permissionsClaim = User?.FindFirstValue("permissions");
		if (string.IsNullOrEmpty(permissionsClaim))
			return Array.Empty<string>();

		try
		{
			return JsonSerializer.Deserialize<string[]>(permissionsClaim) ?? Array.Empty<string>();
		}
		catch
		{
			return Array.Empty<string>();
		}
	}

	public bool HasPermission(string permission)
	{
		return GetPermissions().Contains(permission);
	}
}