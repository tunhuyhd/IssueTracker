using IssueTracker.Domain.Entities.Enum;
using IssueTracker.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Controllers.Examples;

/// <summary>
/// Example controller demonstrating various authorization scenarios
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger in production
public class AuthorizationExampleController : BaseApiController
{
	// ========================================
	// 1. Public Endpoint - No Authorization
	// ========================================
	
	[HttpGet("public/health")]
	public IActionResult HealthCheck()
	{
		return Ok(new { status = "healthy", message = "Anyone can access this" });
	}

	// ========================================
	// 2. Authenticated Only
	// ========================================
	
	[HttpGet("authenticated/profile")]
	[MustBeAuthenticated]
	public IActionResult GetProfile()
	{
		return Ok(new { message = "Only authenticated users can access this" });
	}

	// ========================================
	// 3. Role-Based Authorization
	// ========================================
	
	[HttpGet("admin/dashboard")]
	[MustHaveRole(RoleCode.Admin)]
	public IActionResult GetAdminDashboard()
	{
		return Ok(new { message = "Only ADMIN role can access this" });
	}

	[HttpGet("admin-or-user/dashboard")]
	[MustHaveRole(RoleCode.Admin, RoleCode.User)]
	public IActionResult GetGeneralDashboard()
	{
		return Ok(new { message = "ADMIN or USER role can access this" });
	}

	// ========================================
	// 4. Permission-Based Authorization (Recommended)
	// ========================================
	
	[HttpPost("users")]
	[MustHavePermission(PermissionCode.UserManage)]
	public IActionResult CreateUser()
	{
		return Ok(new { message = "Must have USER_MANAGE permission" });
	}

	[HttpGet("users")]
	[MustHavePermission(PermissionCode.UserManage, PermissionCode.ViewAllProjects)]
	public IActionResult GetUsers()
	{
		return Ok(new { message = "Must have USER_MANAGE OR VIEW_ALL_PROJECTS permission" });
	}

	[HttpDelete("users/{id}")]
	[MustHavePermission(
		PermissionCode.UserManage, 
		PermissionCode.SystemSettings, 
		RequireAll = true)]
	public IActionResult DeleteUser(Guid id)
	{
		return Ok(new { message = "Must have BOTH USER_MANAGE AND SYSTEM_SETTINGS permissions" });
	}

	// ========================================
	// 5. Combined Role and Permission
	// ========================================
	
	[HttpPost("critical/operation")]
	[MustHaveRole(RoleCode.Admin)]
	[MustHavePermission(PermissionCode.SystemSettings)]
	public IActionResult CriticalOperation()
	{
		return Ok(new { message = "Must be ADMIN AND have SYSTEM_SETTINGS permission" });
	}

	// ========================================
	// 6. Project Permissions
	// ========================================
	
	[HttpPost("projects")]
	[MustHavePermission(ProjectPermissionCode.ProjectEdit)]
	public IActionResult CreateProject()
	{
		return Ok(new { message = "Must have PROJECT_EDIT permission" });
	}

	[HttpGet("projects")]
	[MustHavePermission(
		ProjectPermissionCode.ViewProject,
		PermissionCode.ViewAllProjects)]
	public IActionResult GetProjects()
	{
		return Ok(new { message = "Must have VIEW_PROJECT OR VIEW_ALL_PROJECTS permission" });
	}

	// ========================================
	// 7. Controller-Level Authorization
	// ========================================
	
	/// <summary>
	/// All endpoints in this nested controller require ADMIN role
	/// </summary>
	[MustHaveRole(RoleCode.Admin)]
	public class AdminOnlyController : BaseApiController
	{
		[HttpGet("settings")]
		public IActionResult GetSettings()
		{
			return Ok(new { message = "Inherited ADMIN role requirement" });
		}

		[HttpPost("settings")]
		[MustHavePermission(PermissionCode.SystemSettings)]
		public IActionResult UpdateSettings()
		{
			return Ok(new { message = "Need ADMIN role + SYSTEM_SETTINGS permission" });
		}
	}
}
