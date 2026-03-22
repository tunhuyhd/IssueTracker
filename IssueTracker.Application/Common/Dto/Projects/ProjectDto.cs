using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.Projects;

public class ProjectDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } 
	public bool IsEnabled { get; set; }
}
