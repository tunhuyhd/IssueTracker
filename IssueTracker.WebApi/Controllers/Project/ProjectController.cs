using IssueTracker.Application.Projects.Commands;
using IssueTracker.Application.Projects.Queries;
using IssueTracker.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Controllers.Project;

public class ProjectController : BaseApiController
{
	[HttpPost]
	[MustBeAuthenticated]
	public async Task<IActionResult> CreateProject(CreateProjectCommand command)
	{
		var result = await Mediator.Send(command);
		return Ok(result);
	}

	[HttpGet]
	[MustBeAuthenticated]
	public async Task<IActionResult> GetMyProjects([FromQuery] GetMyProjectQuery query)
	{
		var result = await Mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("{id:guid}")]
	[MustBeAuthenticated]
	public async Task<IActionResult> GetProjectById(Guid id)
	{
		var result = await Mediator.Send(new GetProjectByIdQuery { Id = id });
		return Ok(result);
	}

	[HttpPut("{id:guid}")]
	[MustBeAuthenticated]
	public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectCommand command)
	{
		if (command == null) return BadRequest();
		command.Id = id;
		var result = await Mediator.Send(command);
		return Ok(result);
	}

	[HttpPut("{id:guid}/toggle")]
	[MustBeAuthenticated]
	public async Task<IActionResult> ToggleEnableProject(Guid id)
	{
		var result = await Mediator.Send(new ToggleEnableProjectCommand { Id = id });
		return Ok(result);
	}

}
