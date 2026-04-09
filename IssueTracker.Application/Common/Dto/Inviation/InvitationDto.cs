using IssueTracker.Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Dto.Inviation;

public class InvitationDto
{
	public Guid Id { get; set; }
	public Guid ProjectId { get; set; }
	public Guid SenderId { get; set; }
	public string SenderEmail { get; set; } = string.Empty;
	public string SenderName { get; set; }
	public string RecipientEmail { get; set; } = string.Empty;
	public StatusOfInvitation StatusOfInvitation { get; set; }
	public Guid ProjectRoleId { get; set; }
	public string ProjectRoleName { get; set; } = string.Empty;
}
