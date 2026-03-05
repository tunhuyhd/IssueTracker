using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Extensions;
using IssueTracker.Domain.Entities.Enum;
using MediatR;

namespace IssueTracker.Application.Examples;

/// <summary>
/// Example command handler demonstrating project-level authorization in business logic
/// </summary>
public class ProjectCommandExamples
{
	// ========================================
	// Example 1: Simple Permission Check
	// ========================================

	public class UpdateProjectCommand : IRequest<bool>
	{
		public Guid ProjectId { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, bool>
	{
		private readonly IProjectAuthorizationService _projectAuth;

		public UpdateProjectCommandHandler(IProjectAuthorizationService projectAuth)
		{
			_projectAuth = projectAuth;
		}

		public async Task<bool> Handle(UpdateProjectCommand request, CancellationToken ct)
		{
			// Check if user has PROJECT_EDIT permission
			await _projectAuth.EnsureHasProjectPermissionAsync(
				request.ProjectId,
				ProjectPermissionCode.ProjectEdit,
				ct);

			// Business logic here...
			return true;
		}
	}

	// ========================================
	// Example 2: Using Extension Methods
	// ========================================

	public class CreateIssueCommand : IRequest<Guid>
	{
		public Guid ProjectId { get; set; }
		public string Title { get; set; } = string.Empty;
	}

	public class CreateIssueCommandHandler : IRequestHandler<CreateIssueCommand, Guid>
	{
		private readonly IProjectAuthorizationService _projectAuth;

		public CreateIssueCommandHandler(IProjectAuthorizationService projectAuth)
		{
			_projectAuth = projectAuth;
		}

		public async Task<Guid> Handle(CreateIssueCommand request, CancellationToken ct)
		{
			// Use extension method for cleaner code
			await _projectAuth.EnsureCanManageIssuesAsync(request.ProjectId, ct);

			// Business logic...
			return Guid.NewGuid();
		}
	}

	// ========================================
	// Example 3: Conditional Logic Based on Permissions
	// ========================================

	public class GetProjectDataCommand : IRequest<ProjectDataDto>
	{
		public Guid ProjectId { get; set; }
	}

	public class GetProjectDataCommandHandler : IRequestHandler<GetProjectDataCommand, ProjectDataDto>
	{
		private readonly IProjectAuthorizationService _projectAuth;

		public GetProjectDataCommandHandler(IProjectAuthorizationService projectAuth)
		{
			_projectAuth = projectAuth;
		}

		public async Task<ProjectDataDto> Handle(GetProjectDataCommand request, CancellationToken ct)
		{
			// Ensure user is at least a project member
			await _projectAuth.EnsureIsProjectMemberAsync(request.ProjectId, ct);

			var result = new ProjectDataDto();

			// Show different data based on permissions
			if (await _projectAuth.CanEditProjectAsync(request.ProjectId, ct))
			{
				// User can edit - show sensitive data
				result.SensitiveData = "Admin data here";
			}

			if (await _projectAuth.CanManageIssuesAsync(request.ProjectId, ct))
			{
				// User can manage issues - show management features
				result.ManagementFeatures = true;
			}

			return result;
		}
	}

	// ========================================
	// Example 4: Multiple Permissions (OR logic)
	// ========================================

	public class ViewProjectReportCommand : IRequest<ReportDto>
	{
		public Guid ProjectId { get; set; }
	}

	public class ViewProjectReportCommandHandler : IRequestHandler<ViewProjectReportCommand, ReportDto>
	{
		private readonly IProjectAuthorizationService _projectAuth;

		public ViewProjectReportCommandHandler(IProjectAuthorizationService projectAuth)
		{
			_projectAuth = projectAuth;
		}

		public async Task<ReportDto> Handle(ViewProjectReportCommand request, CancellationToken ct)
		{
			// User needs PROJECT_EDIT OR ISSUE_MANAGE permission
			await _projectAuth.EnsureHasAnyProjectPermissionAsync(
				request.ProjectId,
				new[] { ProjectPermissionCode.ProjectEdit, ProjectPermissionCode.IssueManage },
				ct);

			// Business logic...
			return new ReportDto();
		}
	}

	// ========================================
	// Example 5: Multiple Permissions (AND logic)
	// ========================================

	public class DeleteProjectCommand : IRequest<bool>
	{
		public Guid ProjectId { get; set; }
	}

	public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, bool>
	{
		private readonly IProjectAuthorizationService _projectAuth;

		public DeleteProjectCommandHandler(IProjectAuthorizationService projectAuth)
		{
			_projectAuth = projectAuth;
		}

		public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken ct)
		{
			// User needs BOTH permissions for dangerous operation
			await _projectAuth.EnsureHasAllProjectPermissionsAsync(
				request.ProjectId,
				new[] { ProjectPermissionCode.ProjectEdit, ProjectPermissionCode.IssueManage },
				ct);

			// Business logic...
			return true;
		}
	}

	// ========================================
	// Example 6: Check Project Ownership
	// ========================================

	public class TransferProjectCommand : IRequest<bool>
	{
		public Guid ProjectId { get; set; }
		public Guid NewOwnerId { get; set; }
	}

	public class TransferProjectCommandHandler : IRequestHandler<TransferProjectCommand, bool>
	{
		private readonly IProjectAuthorizationService _projectAuth;
		private readonly ICurrentUser _currentUser;

		public TransferProjectCommandHandler(
			IProjectAuthorizationService projectAuth,
			ICurrentUser currentUser)
		{
			_projectAuth = projectAuth;
			_currentUser = currentUser;
		}

		public async Task<bool> Handle(TransferProjectCommand request, CancellationToken ct)
		{
			// Only owner or system admin can transfer
			var isOwner = await _projectAuth.IsProjectOwnerAsync(request.ProjectId, ct);
			var isAdmin = _currentUser.IsAdmin();

			if (!isOwner && !isAdmin)
			{
				throw new UnauthorizedAccessException("Only project owner or admin can transfer project");
			}

			// Business logic...
			return true;
		}
	}

	// ========================================
	// Example 7: Combining System and Project Permissions
	// ========================================

	public class ArchiveProjectCommand : IRequest<bool>
	{
		public Guid ProjectId { get; set; }
	}

	public class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, bool>
	{
		private readonly IProjectAuthorizationService _projectAuth;
		private readonly ICurrentUser _currentUser;

		public ArchiveProjectCommandHandler(
			IProjectAuthorizationService projectAuth,
			ICurrentUser currentUser)
		{
			_projectAuth = projectAuth;
			_currentUser = currentUser;
		}

		public async Task<bool> Handle(ArchiveProjectCommand request, CancellationToken ct)
		{
			// Check system-level permission first
			if (!_currentUser.HasPermission(PermissionCode.SystemSettings))
			{
				// If not system admin, check project permission
				await _projectAuth.EnsureHasProjectPermissionAsync(
					request.ProjectId,
					ProjectPermissionCode.ProjectEdit,
					ct);
			}

			// Business logic...
			return true;
		}
	}

	// ========================================
	// Example 8: Get User's Role in Project
	// ========================================

	public class GetUserProjectRoleQuery : IRequest<string?>
	{
		public Guid ProjectId { get; set; }
	}

	public class GetUserProjectRoleQueryHandler : IRequestHandler<GetUserProjectRoleQuery, string?>
	{
		private readonly IProjectAuthorizationService _projectAuth;

		public GetUserProjectRoleQueryHandler(IProjectAuthorizationService projectAuth)
		{
			_projectAuth = projectAuth;
		}

		public async Task<string?> Handle(GetUserProjectRoleQuery request, CancellationToken ct)
		{
			// Get user's role in the project
			var roleCode = await _projectAuth.GetProjectRoleAsync(request.ProjectId, ct);

			return roleCode; // Returns: PROJECT_MANAGER, DEVELOPER, TESTER, etc.
		}
	}

	// ========================================
	// Example 9: Get All User's Permissions in Project
	// ========================================

	public class GetUserProjectPermissionsQuery : IRequest<string[]>
	{
		public Guid ProjectId { get; set; }
	}

	public class GetUserProjectPermissionsQueryHandler : IRequestHandler<GetUserProjectPermissionsQuery, string[]>
	{
		private readonly IProjectAuthorizationService _projectAuth;

		public GetUserProjectPermissionsQueryHandler(IProjectAuthorizationService projectAuth)
		{
			_projectAuth = projectAuth;
		}

		public async Task<string[]> Handle(GetUserProjectPermissionsQuery request, CancellationToken ct)
		{
			// Get all permissions user has in the project
			var permissions = await _projectAuth.GetProjectPermissionsAsync(request.ProjectId, ct);

			return permissions; // Returns: ["PROJECT_EDIT", "ISSUE_MANAGE", "VIEW_PROJECT"]
		}
	}
}

// DTOs for examples
public class ProjectDataDto
{
	public string? SensitiveData { get; set; }
	public bool ManagementFeatures { get; set; }
}

public class ReportDto
{
	public string Data { get; set; } = string.Empty;
}
