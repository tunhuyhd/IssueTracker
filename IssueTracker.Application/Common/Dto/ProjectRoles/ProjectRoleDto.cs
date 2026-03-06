using IssueTracker.Application.Common.Dto.ProjectPermission;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.ProjectRoles;

public class ProjectRoleDto
{
	public Guid Id { get; set; }
	public string Code { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public List<ProjectPermissionDto> Permissions { get; set; } = [];
}
