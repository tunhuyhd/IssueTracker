using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.ProjectPermission;

public class ProjectPermissionDto
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Code { get; set; }
}
