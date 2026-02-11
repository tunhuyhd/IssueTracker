using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto;

public class TokenResponse
{
	public string AccessToken { get; set; } = string.Empty;
	public string RefreshToken { get; set; } = string.Empty;
	public int ExpiresIn { get; set; }
	public string TokenType { get; set; } = "Bearer";
}
