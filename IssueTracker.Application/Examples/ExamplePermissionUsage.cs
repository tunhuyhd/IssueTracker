using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Extensions;
using IssueTracker.Domain.Entities.Enum;
using MediatR;

namespace IssueTracker.Application.Examples;

/// <summary>
/// Example command handler demonstrating permission checking in business logic
/// </summary>
public class ExampleCommandHandler : IRequestHandler<ExampleCommand, ExampleResult>
{
	private readonly ICurrentUser _currentUser;

	public ExampleCommandHandler(ICurrentUser currentUser)
	{
		_currentUser = currentUser;
	}

	public async Task<ExampleResult> Handle(ExampleCommand request, CancellationToken cancellationToken)
	{
		// ========================================
		// Example 1: Simple permission check
		// ========================================
		if (!_currentUser.HasPermission(PermissionCode.UserManage))
		{
			throw new UnauthorizedAccessException("You don't have permission to manage users");
		}

		// ========================================
		// Example 2: Using extension method (cleaner)
		// ========================================
		_currentUser.EnsureHasPermission(PermissionCode.UserManage);

		// ========================================
		// Example 3: Check multiple permissions (OR logic)
		// ========================================
		if (!_currentUser.HasAnyPermission(
			PermissionCode.UserManage,
			PermissionCode.SystemSettings))
		{
			throw new UnauthorizedAccessException("You need either USER_MANAGE or SYSTEM_SETTINGS permission");
		}

		// OR using extension
		_currentUser.EnsureHasAnyPermission(
			PermissionCode.UserManage,
			PermissionCode.SystemSettings);

		// ========================================
		// Example 4: Check multiple permissions (AND logic)
		// ========================================
		if (!_currentUser.HasAllPermissions(
			PermissionCode.UserManage,
			PermissionCode.SystemSettings))
		{
			throw new UnauthorizedAccessException("You need both USER_MANAGE and SYSTEM_SETTINGS permissions");
		}

		// OR using extension
		_currentUser.EnsureHasAllPermissions(
			PermissionCode.UserManage,
			PermissionCode.SystemSettings);

		// ========================================
		// Example 5: Check role
		// ========================================
		if (!_currentUser.IsAdmin())
		{
			throw new UnauthorizedAccessException("Only admins can perform this action");
		}

		// OR using extension
		_currentUser.EnsureIsAdmin();

		// ========================================
		// Example 6: Conditional logic based on permissions
		// ========================================
		var result = new ExampleResult();

		if (_currentUser.CanViewAllProjects())
		{
			// Admin can see all projects
			result.Projects = await GetAllProjects(cancellationToken);
		}
		else
		{
			// Regular user can only see their own projects
			result.Projects = await GetUserProjects(_currentUser.GetUserId(), cancellationToken);
		}

		// ========================================
		// Example 7: Dynamic permission check
		// ========================================
		var requiredPermission = request.IsHighRisk
			? PermissionCode.SystemSettings
			: PermissionCode.UserManage;

		_currentUser.EnsureHasPermission(requiredPermission);

		// ========================================
		// Example 8: Check ownership + permission
		// ========================================
		var resource = await GetResource(request.ResourceId, cancellationToken);
		
		// User can modify if they own it OR have admin permission
		bool canModify = resource.OwnerId == _currentUser.GetUserId() ||
		                 _currentUser.CanViewAllProjects();

		if (!canModify)
		{
			throw new UnauthorizedAccessException("You don't have permission to modify this resource");
		}

		// ========================================
		// Example 9: Combine role and permission checks
		// ========================================
		if (!_currentUser.IsAdmin() && !_currentUser.HasPermission(PermissionCode.UserManage))
		{
			throw new UnauthorizedAccessException("You need to be an admin or have USER_MANAGE permission");
		}

		return result;
	}

	private Task<List<string>> GetAllProjects(CancellationToken cancellationToken)
	{
		// Implementation
		return Task.FromResult(new List<string> { "Project1", "Project2", "Project3" });
	}

	private Task<List<string>> GetUserProjects(Guid userId, CancellationToken cancellationToken)
	{
		// Implementation
		return Task.FromResult(new List<string> { "MyProject1" });
	}

	private Task<ExampleResource> GetResource(Guid resourceId, CancellationToken cancellationToken)
	{
		// Implementation
		return Task.FromResult(new ExampleResource { Id = resourceId, OwnerId = Guid.NewGuid() });
	}
}

public record ExampleCommand : IRequest<ExampleResult>
{
	public bool IsHighRisk { get; init; }
	public Guid ResourceId { get; init; }
}

public class ExampleResult
{
	public List<string> Projects { get; set; } = new();
}

public class ExampleResource
{
	public Guid Id { get; set; }
	public Guid OwnerId { get; set; }
}
