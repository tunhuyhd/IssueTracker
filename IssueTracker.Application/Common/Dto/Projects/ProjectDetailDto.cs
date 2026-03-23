using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.Projects;

public class ProjectDetailDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; }
	public bool IsEnabled { get; set; }

	public DateTime CreatedOn { get; set; }
}
