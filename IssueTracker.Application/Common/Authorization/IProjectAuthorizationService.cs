namespace IssueTracker.Application.Common.Authorization;

/// <summary>
/// Service for project-level authorization
/// </summary>
public interface IProjectAuthorizationService
{
	/// <summary>
	/// Get current user's project role in a specific project
	/// </summary>
	Task<string?> GetProjectRoleAsync(Guid projectId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Get current user's project permissions in a specific project
	/// </summary>
	Task<string[]> GetProjectPermissionsAsync(Guid projectId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Check if current user has specific permission in a project
	/// </summary>
	Task<bool> HasProjectPermissionAsync(Guid projectId, string permissionCode, CancellationToken cancellationToken = default);

	/// <summary>
	/// Check if current user has any of the specified permissions in a project
	/// </summary>
	Task<bool> HasAnyProjectPermissionAsync(Guid projectId, string[] permissionCodes, CancellationToken cancellationToken = default);

	/// <summary>
	/// Check if current user has all of the specified permissions in a project
	/// </summary>
	Task<bool> HasAllProjectPermissionsAsync(Guid projectId, string[] permissionCodes, CancellationToken cancellationToken = default);

	/// <summary>
	/// Check if current user is member of a project
	/// </summary>
	Task<bool> IsProjectMemberAsync(Guid projectId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Check if current user is owner of a project
	/// </summary>
	Task<bool> IsProjectOwnerAsync(Guid projectId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Throw exception if user doesn't have permission in project
	/// </summary>
	Task EnsureHasProjectPermissionAsync(Guid projectId, string permissionCode, CancellationToken cancellationToken = default);

	/// <summary>
	/// Throw exception if user is not a project member
	/// </summary>
	Task EnsureIsProjectMemberAsync(Guid projectId, CancellationToken cancellationToken = default);
}
