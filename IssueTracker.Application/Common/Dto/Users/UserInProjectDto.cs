using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.Users;

public class UserInProjectDto
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public string Email { get; set; }
	public string ProjectRoleCode { get; set; }

}
