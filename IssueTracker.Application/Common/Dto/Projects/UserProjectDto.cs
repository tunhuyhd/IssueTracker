using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.Projects;

public class UserProjectDto
{
	public Guid UserId { get; set; }
	public Guid ProjectId { get; set; }

	public string ProjectName { get; set; }
	public string UserName { get; set; }

	public string UserEmail { get; set; }
}
