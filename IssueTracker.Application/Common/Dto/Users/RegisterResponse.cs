using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.User;

public class RegisterResponse
{
	public Guid UserId { get; set; }
	public string Username { get; set; } = string.Empty;
}
