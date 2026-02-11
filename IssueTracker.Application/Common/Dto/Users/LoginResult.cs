using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.Users;

public class LoginResult
{
	public string AccessToken { get; set; } = string.Empty;
	public string RefreshToken { get; set; } = string.Empty;
	public DateTime ExpiresAt { get; set; }

	public LoginResult() { }

	public LoginResult(string accessToken, string refreshToken = "", DateTime? expiresAt = null)
	{
		AccessToken = accessToken;
		RefreshToken = refreshToken;
		ExpiresAt = expiresAt ?? DateTime.UtcNow.AddHours(1);
	}

}
