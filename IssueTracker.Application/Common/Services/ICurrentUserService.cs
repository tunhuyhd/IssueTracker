using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Entities;

namespace IssueTracker.Application.Common.Services;

public interface ICurrentUserService
{
	/// <summary>
	/// Get current user ID from JWT claims
	/// </summary>
	Guid GetUserId();

	/// <summary>
	/// Get current username from JWT claims
	/// </summary>
	string GetUsername();

	/// <summary>
	/// Get current user email from JWT claims
	/// </summary>
	string GetUserEmail();

	/// <summary>
	/// Check if user is authenticated
	/// </summary>
	bool IsAuthenticated();

	/// <summary>
	/// Get full user entity from database with includes
	/// </summary>
	Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Get full user entity with role and permissions
	/// </summary>
	Task<User?> GetCurrentUserWithRoleAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Get full user entity with projects
	/// </summary>
	Task<User?> GetCurrentUserWithProjectsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Check if current user has specific permission
	/// </summary>
	bool HasPermission(string permissionCode);

	/// <summary>
	/// Check if current user has specific role
	/// </summary>
	bool HasRole(string roleCode);

	/// <summary>
	/// Check if current user is member of specific project
	/// </summary>
	Task<bool> IsMemberOfProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Check if current user is owner of specific project
	/// </summary>
	Task<bool> IsProjectOwnerAsync(Guid projectId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Get current user's role in specific project
	/// </summary>
	Task<ProjectRole?> GetProjectRoleAsync(Guid projectId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Get all projects of current user
	/// </summary>
	Task<List<Project>> GetUserProjectsAsync(CancellationToken cancellationToken = default);
}
