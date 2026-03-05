using IssueTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IssueTracker.Domain.Entities;

[Table("projects")]
public class Project : AuditableEntity, IAggregateRoot
{
	[Column("name")]
	public string Name { get; set; } = string.Empty;

	[Column("description")]
	public string Description { get; set; } = string.Empty;

	[Column("owner_id")]
	public Guid OwnerId { get; set; }

	public User Owner { get; set; } = null!;

	[Column("is_enabled")]

	public bool IsEnabled { get; set; } = true;

	public List<UserProject> UserProjects { get; set; } = [];

	public Project() { }

}
