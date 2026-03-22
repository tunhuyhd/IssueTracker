using IssueTracker.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Domain.Entities;

[Table("user_projects")]
public class UserProject : AuditableEntity, IAggregateRoot
{
	[Column("user_id")]
	public Guid UserId { get; set; }
	public User User { get; set; } = null!;

	[Column("project_id")]
	public Guid ProjectId { get; set; }
	public Project Project { get; set; } = null!;

	[Column("project_role_id")]
	public Guid ProjectRoleId { get; set; }
	public ProjectRole ProjectRole { get; set; } = null!;
}
