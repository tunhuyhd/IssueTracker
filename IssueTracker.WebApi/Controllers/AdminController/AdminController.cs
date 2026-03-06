using IssueTracker.Application.Admin.Commands;
using IssueTracker.Application.Admin.Queries;
using IssueTracker.Domain.Entities.Enum;
using IssueTracker.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Controllers.AdminController;

[MustHaveRole(RoleCode.Admin)]
public class AdminController : BaseApiController
{
	[HttpPost("project-roles")]
	[MustHavePermission(PermissionCode.SystemSettings)]
	public async Task<IActionResult> CreateProjectRole(AddProjectRoleCommand command)
	{
		var result = await Mediator.Send(command);
		return Ok(result);
	}

	[HttpPost("project-permissions")]
	[MustHavePermission(PermissionCode.SystemSettings)]
	public async Task<IActionResult> CreateProjectPermission(AddProjectPermissionCommand command)
	{
		var result = await Mediator.Send(command);
		return Ok(result);
	}

	[HttpGet("roles")]
	[MustHavePermission(PermissionCode.SystemSettings)]
	public async Task<IActionResult> GetRoles()
	{
		var result = await Mediator.Send(new GetListRoleQuery());
		return Ok(result);
	}

	[HttpGet("roles/{id}")]
	[MustHavePermission(PermissionCode.SystemSettings)]
	public async Task<IActionResult> GetRole(Guid id)
	{
		var result = await Mediator.Send(new GetRoleQuery { Id = id });
		return Ok(result);
	}

	[HttpPut("project-roles/{id:guid}")]
	[MustHavePermission(PermissionCode.SystemSettings)]
	public async Task<IActionResult> UpdateProjectRole(Guid id, [FromBody] UpdateProjectRoleCommand command)
	{
		command.Id = id;
		var result = await Mediator.Send(command);
		return Ok(result);
	}

	[HttpGet("permissions")]
	[MustHavePermission(PermissionCode.SystemSettings)]
	public async Task<IActionResult> GetPermissions()
	{
		var result = await Mediator.Send(new GetListPermissionQuery());
		return Ok(result);
	}

	[HttpDelete("project-permissions/{id:guid}")]
	[MustHavePermission(PermissionCode.SystemSettings)]
	public async Task<IActionResult> DeleteProjectPermission(Guid id)
	{
		await Mediator.Send(new DeleteProjectPermissionCommand { Id = id });
		return NoContent();
	}

	[HttpPut("users/{userId:guid}/role")]
	[MustHavePermission(PermissionCode.UserManage)]
	public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleCommand command)
	{
		command.UserId = userId;  // Bind userId from route
		var result = await Mediator.Send(command);
		return Ok(result);
	}

	[HttpGet("list-users")]
	[MustHavePermission(PermissionCode.UserManage)]
	public async Task<IActionResult> GetUsers([FromQuery] GetListUserQuery query)
	{
		var result = await Mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("users/{userId:guid}")]
	[MustHavePermission(PermissionCode.UserManage)]
	public async Task<IActionResult> GetUser(Guid userId)
	{
		var result = await Mediator.Send(new GetUserByIdQuery { UserId = userId });
		return Ok(result);
	}
}
