using IssueTracker.Application.Common.Authorization;
using IssueTracker.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Infrastructure.Auth;

public class ProjectAuthorizationService : IProjectAuthorizationService
{
	private readonly ApplicationDbContext _context;
	private readonly ICurrentUser _currentUser;

	public ProjectAuthorizationService(
		ApplicationDbContext context,
		ICurrentUser currentUser)
	{
		_context = context;
		_currentUser = currentUser;
	}

	public async Task<string?> GetProjectRoleAsync(Guid projectId, CancellationToken cancellationToken = default)
	{
		var userId = _currentUser.GetUserId();
		if (userId == Guid.Empty)
			return null;

		var userProject = await _context.UserProjects
			.Include(up => up.ProjectRole)
			.FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId, cancellationToken);

		return userProject?.ProjectRole?.Code;
	}

	public async Task<string[]> GetProjectPermissionsAsync(Guid projectId, CancellationToken cancellationToken = default)
	{
		var userId = _currentUser.GetUserId();
		if (userId == Guid.Empty)
			return Array.Empty<string>();

		// If user is system admin, grant all project permissions (excluding soft deleted)
		if (_currentUser.GetRoleCode() == Domain.Entities.Enum.RoleCode.Admin)
		{
			var allPermissions = await _context.ProjectPermissions
				.Where(pp => pp.DeletedOn == null)
				.Select(pp => pp.Code)
				.ToArrayAsync(cancellationToken);
			return allPermissions;
		}

		// Get user's project role and permissions (excluding soft deleted junction and permissions)
		var permissions = await _context.UserProjects
			.Where(up => up.UserId == userId && up.ProjectId == projectId)
			.SelectMany(up => up.ProjectRole.ProjectRolePermissions)
			.Where(prp => prp.DeletedOn == null && prp.ProjectPermission.DeletedOn == null)
			.Select(prp => prp.ProjectPermission.Code)
			.Distinct()
			.ToArrayAsync(cancellationToken);

		return permissions;
	}

	public async Task<bool> HasProjectPermissionAsync(Guid projectId, string permissionCode, CancellationToken cancellationToken = default)
	{
		// System admin has all project permissions
		if (_currentUser.GetRoleCode() == Domain.Entities.Enum.RoleCode.Admin)
			return true;

		var permissions = await GetProjectPermissionsAsync(projectId, cancellationToken);
		return permissions.Contains(permissionCode);
	}

	public async Task<bool> HasAnyProjectPermissionAsync(Guid projectId, string[] permissionCodes, CancellationToken cancellationToken = default)
	{
		// System admin has all project permissions
		if (_currentUser.GetRoleCode() == Domain.Entities.Enum.RoleCode.Admin)
			return true;

		var permissions = await GetProjectPermissionsAsync(projectId, cancellationToken);
		return permissionCodes.Any(code => permissions.Contains(code));
	}

	public async Task<bool> HasAllProjectPermissionsAsync(Guid projectId, string[] permissionCodes, CancellationToken cancellationToken = default)
	{
		// System admin has all project permissions
		if (_currentUser.GetRoleCode() == Domain.Entities.Enum.RoleCode.Admin)
			return true;

		var permissions = await GetProjectPermissionsAsync(projectId, cancellationToken);
		return permissionCodes.All(code => permissions.Contains(code));
	}

	public async Task<bool> IsProjectMemberAsync(Guid projectId, CancellationToken cancellationToken = default)
	{
		var userId = _currentUser.GetUserId();
		if (userId == Guid.Empty)
			return false;

		// System admin can access all projects
		if (_currentUser.GetRoleCode() == Domain.Entities.Enum.RoleCode.Admin)
			return true;

		return await _context.UserProjects
			.AnyAsync(up => up.UserId == userId && up.ProjectId == projectId, cancellationToken);
	}

	public async Task<bool> IsProjectOwnerAsync(Guid projectId, CancellationToken cancellationToken = default)
	{
		var userId = _currentUser.GetUserId();
		if (userId == Guid.Empty)
			return false;

		var project = await _context.Projects
			.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

		if (project == null)
			return false;

		return project.OwnerId == userId;
	}

	public async Task EnsureHasProjectPermissionAsync(Guid projectId, string permissionCode, CancellationToken cancellationToken = default)
	{
		if (!await HasProjectPermissionAsync(projectId, permissionCode, cancellationToken))
		{
			throw new UnauthorizedAccessException($"User does not have permission '{permissionCode}' in project {projectId}");
		}
	}

	public async Task EnsureIsProjectMemberAsync(Guid projectId, CancellationToken cancellationToken = default)
	{
		if (!await IsProjectMemberAsync(projectId, cancellationToken))
		{
			throw new UnauthorizedAccessException($"User is not a member of project {projectId}");
		}
	}
}
