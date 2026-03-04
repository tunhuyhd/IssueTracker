using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace IssueTracker.Application.Common.Dto;

public class TokenResponse
{
	public string AccessToken { get; set; } = string.Empty;
	public int ExpiresIn { get; set; }
	public string TokenType { get; set; } = "Bearer";

	// Không serialize ra JSON response nhưng vẫn accessible trong code
	[JsonIgnore]
	public string RefreshToken { get; set; } = string.Empty;
}
