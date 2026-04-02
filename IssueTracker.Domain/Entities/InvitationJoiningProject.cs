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

	private InvitationJoiningProject() { }
}
