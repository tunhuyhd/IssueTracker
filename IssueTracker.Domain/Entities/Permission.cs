using IssueTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IssueTracker.Domain.Entities;

[Table("permissions")]
public class Permission : AuditableEntity, IAggregateRoot
{
	[Column("code")]
	public string Code { get; set; } = string.Empty;

	[Column("name")]
	public string Name { get; set; } = string.Empty;

	public List<Role> Roles { get; set; } = [];
}

