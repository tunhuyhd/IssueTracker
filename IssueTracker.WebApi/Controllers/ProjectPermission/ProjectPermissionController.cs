using IssueTracker.Application.ProjectRoles.Queries;
using IssueTracker.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Controllers.ProjectPermission;

public class ProjectPermissionController : BaseApiController
{
	[HttpGet("")]
	[MustBeAuthenticated]
	public async Task<IActionResult> GetProjectPermissions()
	{
		var result = await Mediator.Send(new GetListProjetPermissionQuerry());
		return Ok(result);
	}
}
