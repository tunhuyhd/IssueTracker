using IssueTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IssueTracker.Domain.Entities;

[Table("roles")]
public class Role : AuditableEntity, IAggregateRoot
{
	[Column("code")]
	public string Code { get; set; } = string.Empty;

	[Column("description")]
	public string Description { get; set; } = string.Empty;

	public List<Permission> Permissions { get; set; } = [];
}
