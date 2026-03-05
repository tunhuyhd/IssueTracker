using IssueTracker.Domain.Entities.Enum;
using IssueTracker.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Controllers.Examples;

/// <summary>
/// Example controller demonstrating project-level authorization
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger in production
[Route("api/examples/projects")]
public class ProjectAuthorizationExampleController : BaseApiController
{
	// ========================================
	// 1. Project Member Only
	// ========================================

	/// <summary>
	/// Only project members can access
	/// </summary>
	[HttpGet("{projectId}/info")]
	[MustBeProjectMember]
	public IActionResult GetProjectInfo(Guid projectId)
	{
		return Ok(new
		{
			message = "Only project members can see this",
			projectId
		});
	}

	// ========================================
	// 2. Project Permission - Single
	// ========================================

	/// <summary>
	/// Requires PROJECT_EDIT permission in the project
	/// </summary>
	[HttpPut("{projectId}")]
	[MustHaveProjectPermission(ProjectPermissionCode.ProjectEdit)]
	public IActionResult UpdateProject(Guid projectId)
	{
		return Ok(new
		{
			message = "User has PROJECT_EDIT permission",
			projectId
		});
	}

	/// <summary>
	/// Requires ISSUE_MANAGE permission in the project
	/// </summary>
	[HttpPost("{projectId}/issues")]
	[MustHaveProjectPermission(ProjectPermissionCode.IssueManage)]
	public IActionResult CreateIssue(Guid projectId)
	{
		return Ok(new
		{
			message = "User has ISSUE_MANAGE permission",
			projectId
		});
	}

	// ========================================
	// 3. Project Permission - Multiple (OR logic)
	// ========================================

	/// <summary>
	/// Requires PROJECT_EDIT OR ISSUE_MANAGE permission
	/// </summary>
	[HttpGet("{projectId}/dashboard")]
	[MustHaveProjectPermission(
		ProjectPermissionCode.ProjectEdit,
		ProjectPermissionCode.IssueManage)]
	public IActionResult GetDashboard(Guid projectId)
	{
		return Ok(new
		{
			message = "User has PROJECT_EDIT OR ISSUE_MANAGE permission",
			projectId
		});
	}

	// ========================================
	// 4. Project Permission - Multiple (AND logic)
	// ========================================

	/// <summary>
	/// Requires BOTH PROJECT_EDIT AND ISSUE_MANAGE permissions
	/// </summary>
	[HttpDelete("{projectId}")]
	[MustHaveProjectPermission(
		ProjectPermissionCode.ProjectEdit,
		ProjectPermissionCode.IssueManage,
		RequireAll = true)]
	public IActionResult DeleteProject(Guid projectId)
	{
		return Ok(new
		{
			message = "User has BOTH PROJECT_EDIT AND ISSUE_MANAGE permissions",
			projectId
		});
	}

	// ========================================
	// 5. Combining System and Project Permissions
	// ========================================

	/// <summary>
	/// Requires Admin role AND project membership
	/// </summary>
	[HttpPost("{projectId}/advanced-settings")]
	[MustHaveRole(RoleCode.Admin)]
	[MustBeProjectMember]
	public IActionResult UpdateAdvancedSettings(Guid projectId)
	{
		return Ok(new
		{
			message = "User is Admin AND project member",
			projectId
		});
	}

	/// <summary>
	/// Requires system permission AND project permission
	/// </summary>
	[HttpPost("{projectId}/sync")]
	[MustHavePermission(PermissionCode.SystemSettings)]
	[MustHaveProjectPermission(ProjectPermissionCode.ProjectEdit)]
	public IActionResult SyncProject(Guid projectId)
	{
		return Ok(new
		{
			message = "User has SYSTEM_SETTINGS AND PROJECT_EDIT permissions",
			projectId
		});
	}

	// ========================================
	// 6. Custom Route Parameter Name
	// ========================================

	/// <summary>
	/// When projectId has different parameter name
	/// </summary>
	[HttpGet("project/{id}/settings")]
	[MustHaveProjectPermission(ProjectPermissionCode.ProjectEdit, "id")]
	public IActionResult GetSettings(Guid id)
	{
		return Ok(new
		{
			message = "Custom parameter name 'id' instead of 'projectId'",
			projectId = id
		});
	}

	// ========================================
	// 7. Nested Routes
	// ========================================

	/// <summary>
	/// Project permission in nested routes
	/// </summary>
	[HttpGet("{projectId}/issues/{issueId}")]
	[MustHaveProjectPermission(ProjectPermissionCode.ViewProject)]
	public IActionResult GetIssue(Guid projectId, Guid issueId)
	{
		return Ok(new
		{
			message = "User has VIEW_PROJECT permission",
			projectId,
			issueId
		});
	}

	// ========================================
	// 8. Read-only Access
	// ========================================

	/// <summary>
	/// Only requires VIEW_PROJECT permission (read-only)
	/// </summary>
	[HttpGet("{projectId}/reports")]
	[MustHaveProjectPermission(ProjectPermissionCode.ViewProject)]
	public IActionResult GetReports(Guid projectId)
	{
		return Ok(new
		{
			message = "User has VIEW_PROJECT permission (read-only)",
			projectId
		});
	}
}
