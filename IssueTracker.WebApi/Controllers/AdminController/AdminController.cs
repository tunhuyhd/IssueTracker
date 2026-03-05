using IssueTracker.Application.Admin.Commands;
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
}
