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
}
