using IssueTracker.Application.Common.Services;
using IssueTracker.Domain.Entities;

namespace IssueTracker.Application.Common.Extensions;

public static class CurrentUserExtensions
{
	/// <summary>
	/// Get current user or throw exception if not found
	/// </summary>
	public static async Task<User> GetCurrentUserOrThrowAsync(
		this ICurrentUserService currentUserService,
		CancellationToken cancellationToken = default)
	{
		var user = await currentUserService.GetCurrentUserAsync(cancellationToken);
		if (user == null)
			throw new UnauthorizedAccessException("User not found or not authenticated");

		return user;
	}

	/// <summary>
	/// Get current user with role or throw exception if not found
	/// </summary>
	public static async Task<User> GetCurrentUserWithRoleOrThrowAsync(
		this ICurrentUserService currentUserService,
		CancellationToken cancellationToken = default)
	{
		var user = await currentUserService.GetCurrentUserWithRoleAsync(cancellationToken);
		if (user == null)
			throw new UnauthorizedAccessException("User not found or not authenticated");

		return user;
	}

	/// <summary>
	/// Ensure user has permission or throw exception
	/// </summary>
	public static void EnsureHasPermission(
		this ICurrentUserService currentUserService,
		string permissionCode,
		string? errorMessage = null)
	{
		if (!currentUserService.HasPermission(permissionCode))
		{
			throw new UnauthorizedAccessException(
				errorMessage ?? $"User does not have permission: {permissionCode}");
		}
	}

	/// <summary>
	/// Ensure user has role or throw exception
	/// </summary>
	public static void EnsureHasRole(
		this ICurrentUserService currentUserService,
		string roleCode,
		string? errorMessage = null)
	{
		if (!currentUserService.HasRole(roleCode))
		{
			throw new UnauthorizedAccessException(
				errorMessage ?? $"User does not have role: {roleCode}");
		}
	}

	/// <summary>
	/// Ensure user is member of project or throw exception
	/// </summary>
	public static async Task EnsureIsMemberOfProjectAsync(
		this ICurrentUserService currentUserService,
		Guid projectId,
		CancellationToken cancellationToken = default)
	{
		if (!await currentUserService.IsMemberOfProjectAsync(projectId, cancellationToken))
		{
			throw new UnauthorizedAccessException("User is not a member of this project");
		}
	}

	/// <summary>
	/// Ensure user is owner of project or throw exception
	/// </summary>
	public static async Task EnsureIsProjectOwnerAsync(
		this ICurrentUserService currentUserService,
		Guid projectId,
		CancellationToken cancellationToken = default)
	{
		if (!await currentUserService.IsProjectOwnerAsync(projectId, cancellationToken))
		{
			throw new UnauthorizedAccessException("User is not the owner of this project");
		}
	}

	/// <summary>
	/// Check if user has any of the specified permissions
	/// </summary>
	public static bool HasAnyPermission(
		this ICurrentUserService currentUserService,
		params string[] permissionCodes)
	{
		return permissionCodes.Any(currentUserService.HasPermission);
	}

	/// <summary>
	/// Check if user has all of the specified permissions
	/// </summary>
	public static bool HasAllPermissions(
		this ICurrentUserService currentUserService,
		params string[] permissionCodes)
	{
		return permissionCodes.All(currentUserService.HasPermission);
	}
}
