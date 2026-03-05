using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Entities.Enum;

namespace IssueTracker.Application.Common.Extensions;

/// <summary>
/// Extension methods for project-level authorization in business logic
/// </summary>
public static class ProjectAuthorizationExtensions
{
	/// <summary>
	/// Throw exception if user doesn't have project permission
	/// </summary>
	public static async Task EnsureHasProjectPermissionAsync(
		this IProjectAuthorizationService service,
		Guid projectId,
		string permissionCode,
		CancellationToken cancellationToken = default,
		string? errorMessage = null)
	{
		if (!await service.HasProjectPermissionAsync(projectId, permissionCode, cancellationToken))
		{
			throw new UnauthorizedAccessException(
				errorMessage ?? $"User does not have permission '{permissionCode}' in project {projectId}");
		}
	}

	/// <summary>
	/// Throw exception if user doesn't have any of the project permissions
	/// </summary>
	public static async Task EnsureHasAnyProjectPermissionAsync(
		this IProjectAuthorizationService service,
		Guid projectId,
		string[] permissionCodes,
		CancellationToken cancellationToken = default,
		string? errorMessage = null)
	{
		if (!await service.HasAnyProjectPermissionAsync(projectId, permissionCodes, cancellationToken))
		{
			var required = string.Join(", ", permissionCodes);
			throw new UnauthorizedAccessException(
				errorMessage ?? $"User does not have any of required permissions in project {projectId}: {required}");
		}
	}

	/// <summary>
	/// Throw exception if user doesn't have all of the project permissions
	/// </summary>
	public static async Task EnsureHasAllProjectPermissionsAsync(
		this IProjectAuthorizationService service,
		Guid projectId,
		string[] permissionCodes,
		CancellationToken cancellationToken = default,
		string? errorMessage = null)
	{
		if (!await service.HasAllProjectPermissionsAsync(projectId, permissionCodes, cancellationToken))
		{
			var required = string.Join(", ", permissionCodes);
			throw new UnauthorizedAccessException(
				errorMessage ?? $"User does not have all required permissions in project {projectId}: {required}");
		}
	}

	/// <summary>
	/// Check if user can edit project
	/// </summary>
	public static async Task<bool> CanEditProjectAsync(
		this IProjectAuthorizationService service,
		Guid projectId,
		CancellationToken cancellationToken = default)
	{
		return await service.HasProjectPermissionAsync(
			projectId,
			ProjectPermissionCode.ProjectEdit,
			cancellationToken);
	}

	/// <summary>
	/// Check if user can manage issues in project
	/// </summary>
	public static async Task<bool> CanManageIssuesAsync(
		this IProjectAuthorizationService service,
		Guid projectId,
		CancellationToken cancellationToken = default)
	{
		return await service.HasProjectPermissionAsync(
			projectId,
			ProjectPermissionCode.IssueManage,
			cancellationToken);
	}

	/// <summary>
	/// Check if user can view project
	/// </summary>
	public static async Task<bool> CanViewProjectAsync(
		this IProjectAuthorizationService service,
		Guid projectId,
		CancellationToken cancellationToken = default)
	{
		return await service.HasProjectPermissionAsync(
			projectId,
			ProjectPermissionCode.ViewProject,
			cancellationToken);
	}

	/// <summary>
	/// Ensure user can edit project or throw exception
	/// </summary>
	public static async Task EnsureCanEditProjectAsync(
		this IProjectAuthorizationService service,
		Guid projectId,
		CancellationToken cancellationToken = default)
	{
		await service.EnsureHasProjectPermissionAsync(
			projectId,
			ProjectPermissionCode.ProjectEdit,
			cancellationToken,
			$"User cannot edit project {projectId}");
	}

	/// <summary>
	/// Ensure user can manage issues or throw exception
	/// </summary>
	public static async Task EnsureCanManageIssuesAsync(
		this IProjectAuthorizationService service,
		Guid projectId,
		CancellationToken cancellationToken = default)
	{
		await service.EnsureHasProjectPermissionAsync(
			projectId,
			ProjectPermissionCode.IssueManage,
			cancellationToken,
			$"User cannot manage issues in project {projectId}");
	}
}
