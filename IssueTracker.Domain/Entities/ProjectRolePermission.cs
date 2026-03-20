using IssueTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IssueTracker.Domain.Entities;

[Table("project_role_permissions")]
public class ProjectRolePermission : AuditableEntity, IAggregateRoot
{
	[Column("project_role_id")]
	public Guid ProjectRoleId { get; set; }
	public ProjectRole ProjectRole { get; set; }

	[Column("project_permission_id")]
	public Guid ProjectPermissionId { get; set; }
	public ProjectPermission ProjectPermission { get; set; }
}
