using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IssueTracker.Domain.Entities;


[Table("invitation_joining_projects")]
public class InvitationJoiningProject : AuditableEntity, IAggregateRoot
{
	[Column("project_id")]
	public Guid ProjectId { get; set; }
	public Project Project { get; set; } = null!;
	[Column("sender_id")]
	public Guid SenderId { get; set; }
	public User Sender { get; set; } = null!;
	[Column("recipient_email")]
	public String RecipientEmail { get; set; }
	[Column("status")]
	public StatusOfInvitation Status { get; set; } = StatusOfInvitation.Pending;

	[Column("project_role_id")]
	public Guid ProjectRoleId { get; set; }
	public ProjectRole ProjectRole { get; set; } = null!;

	private InvitationJoiningProject() { }

	public static InvitationJoiningProject Create(Guid projectId, Guid senderId, string recipientEmail, Guid projectRoleId)
	{
		return new InvitationJoiningProject
		{
			ProjectId = projectId,
			SenderId = senderId,
			RecipientEmail = recipientEmail,
			ProjectRoleId = projectRoleId,
			Status = StatusOfInvitation.Pending
		};
	}
}
