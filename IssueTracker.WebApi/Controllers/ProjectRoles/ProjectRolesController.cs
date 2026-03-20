using IssueTracker.Application.ProjectRoles.Queries;
using IssueTracker.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace IssueTracker.WebApi.Controllers.ProjectRoles;

public class ProjectRolesController : BaseApiController
{
   [HttpGet("project-roles")]
   [MustBeAuthenticated]
   public async Task<IActionResult> GetProjectRoles()
   {
	   var result = await Mediator.Send(new GetListProjectRoleQuery());
	   return Ok(result);
	}

	[HttpGet("project-roles/{id:guid}")]
	[MustBeAuthenticated]
	public async Task<IActionResult> GetProjectRoleById(Guid id)
	{
		var result = await Mediator.Send(new GetProjectRoleByIdQuery { Id = id });
		return Ok(result);
	}

	[HttpGet("project-roles/{id:guid}/available-permissions")]
	[MustBeAuthenticated]
	public async Task<IActionResult> GetAvailablePermissions(Guid id)
	{
		var result = await Mediator.Send(new GetAvailableProjectPermissionsQuery { ProjectRoleId = id });
		return Ok(result);
	}
}
