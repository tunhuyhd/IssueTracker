using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Services;
using IssueTracker.Domain.Entities;
using IssueTracker.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IssueTracker.Infrastructure.Auth;

public class CurrentUserService : ICurrentUserService
{
	private readonly ICurrentUser _currentUser;
	private readonly ApplicationDbContext _dbContext;
	private readonly IMemoryCache _cache;

	public CurrentUserService(
		ICurrentUser currentUser,
		ApplicationDbContext dbContext,
		IMemoryCache cache)
	{
		_currentUser = currentUser;
		_dbContext = dbContext;
		_cache = cache;
	}

	public Guid GetUserId() => _currentUser.GetUserId();

	public string GetUsername() => _currentUser.GetUsername();

	public string GetUserEmail() => _currentUser.GetUserEmail();

	public bool IsAuthenticated() => _currentUser.IsAuthenticated();

	public async Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
	{
		var userId = GetUserId();
		if (userId == Guid.Empty)
			return null;

		var cacheKey = $"user_{userId}";
		
		if (_cache.TryGetValue(cacheKey, out User? cachedUser))
			return cachedUser;

		var user = await _dbContext.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

		if (user != null)
		{
			_cache.Set(cacheKey, user, TimeSpan.FromMinutes(5));
		}

		return user;
	}

	public async Task<User?> GetCurrentUserWithRoleAsync(CancellationToken cancellationToken = default)
	{
		var userId = GetUserId();
		if (userId == Guid.Empty)
			return null;

		var cacheKey = $"user_with_role_{userId}";

		if (_cache.TryGetValue(cacheKey, out User? cachedUser))
			return cachedUser;

		var user = await _dbContext.Users
			.AsNoTracking()
			.Include(u => u.Role)
				.ThenInclude(r => r.Permissions)
			.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

		if (user != null)
		{
			_cache.Set(cacheKey, user, TimeSpan.FromMinutes(5));
		}

		return user;
	}

	public async Task<User?> GetCurrentUserWithProjectsAsync(CancellationToken cancellationToken = default)
	{
		var userId = GetUserId();
		if (userId == Guid.Empty)
			return null;

		return await _dbContext.Users
			.AsNoTracking()
			.Include(u => u.UserProjects)
				.ThenInclude(up => up.Project)
			.Include(u => u.UserProjects)
				.ThenInclude(up => up.ProjectRole)
			.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);
	}

	public bool HasPermission(string permissionCode)
	{
		return _currentUser.HasPermission(permissionCode);
	}

	public bool HasRole(string roleCode)
	{
		return _currentUser.GetRoleCode().Equals(roleCode, StringComparison.OrdinalIgnoreCase);
	}

	public async Task<bool> IsMemberOfProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
	{
		var userId = GetUserId();
		if (userId == Guid.Empty)
			return false;

		return await _dbContext.Set<UserProject>()
			.AnyAsync(up => up.UserId == userId && up.ProjectId == projectId, cancellationToken);
	}

	public async Task<bool> IsProjectOwnerAsync(Guid projectId, CancellationToken cancellationToken = default)
	{
		var userId = GetUserId();
		if (userId == Guid.Empty)
			return false;

		var project = await _dbContext.Set<Project>()
			.AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

		return project != null && project.OwnerId == userId.ToString();
	}

	public async Task<ProjectRole?> GetProjectRoleAsync(Guid projectId, CancellationToken cancellationToken = default)
	{
		var userId = GetUserId();
		if (userId == Guid.Empty)
			return null;

		var userProject = await _dbContext.Set<UserProject>()
			.AsNoTracking()
			.Include(up => up.ProjectRole)
				.ThenInclude(pr => pr.ProjectPermissions)
			.FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId, cancellationToken);

		return userProject?.ProjectRole;
	}

	public async Task<List<Project>> GetUserProjectsAsync(CancellationToken cancellationToken = default)
	{
		var userId = GetUserId();
		if (userId == Guid.Empty)
			return [];

		var projects = await _dbContext.Set<UserProject>()
			.AsNoTracking()
			.Where(up => up.UserId == userId)
			.Include(up => up.Project)
			.Include(up => up.ProjectRole)
			.Select(up => up.Project)
			.ToListAsync(cancellationToken);

		return projects;
	}
}
