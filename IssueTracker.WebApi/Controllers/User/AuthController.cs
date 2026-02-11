using IssueTracker.Application.Users.Commands;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Controllers.User;

public class AuthController : BaseApiController
{
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterCommand command)
	{
		var result = await Mediator.Send(command);
		return Ok(result);
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginCommand command)
	{
		var result = await Mediator.Send(command);
		return Ok(result);
	}

}
