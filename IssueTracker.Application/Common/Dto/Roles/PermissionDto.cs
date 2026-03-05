using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.Roles;

public class PermissionDto
{
	public Guid Id { get; set; }
	public string Code { get; set; }
	public string Name { get; set; }
}
