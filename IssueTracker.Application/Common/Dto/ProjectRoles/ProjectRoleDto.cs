using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.ProjectRoles;

public class ProjectRoleDto
{
	public Guid Id { get; set; }
	public string Code { get; set; }
	public string Description { get; set; }
}
