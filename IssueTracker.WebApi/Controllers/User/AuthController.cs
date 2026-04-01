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

		// Set refresh token in HTTPOnly cookie
		SetRefreshTokenCookie(result.RefreshToken);

		return Ok(result);
	}

	[HttpPost("refresh")] 
	public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
	{
		// Get refresh token from cookie
		var refreshToken = Request.Cookies["refreshToken"];

		if (string.IsNullOrEmpty(refreshToken))
			return Unauthorized(new { message = "Refresh token not found" });

		command.RefreshToken = refreshToken;
		var result = await Mediator.Send(command);

		// Set new refresh token in cookie
		SetRefreshTokenCookie(result.RefreshToken);

		return Ok(result);
	}

	[HttpPost("logout")]
	public IActionResult Logout()
	{
		// Clear refresh token cookie
		Response.Cookies.Delete("refreshToken");
		return Ok(new { message = "Logged out successfully" });
	}

	private void SetRefreshTokenCookie(string refreshToken)
	{
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = true, // Chỉ gửi qua HTTPS
			SameSite = SameSiteMode.Strict,
			Expires = DateTimeOffset.UtcNow.AddDays(7)
		};

		Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
	}
}
