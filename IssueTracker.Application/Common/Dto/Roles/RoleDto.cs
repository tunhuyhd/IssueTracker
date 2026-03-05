using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.Roles;

public class RoleDto
{
	public Guid Id { get; set; }
	public string Code { get; set; }
	public string Description { get; set; }

	public List<PermissionDto> Permissions { get; set; } = [];
}
