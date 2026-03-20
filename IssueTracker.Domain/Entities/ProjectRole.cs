using IssueTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IssueTracker.Domain.Entities;

[Table("project_roles")]
public class ProjectRole : AuditableEntity, IAggregateRoot
{
	[Column("code")]
	public string Code { get; set; } = string.Empty;
	[Column("description")]
	public string Description { get; set; } = string.Empty;

	public List<ProjectRolePermission> ProjectRolePermissions { get; set; } = [];

	/// Update project role information
	public void Update(string? code, string? description)
	{
		if (code != null)
			Code = code;
		if (description != null)
			Description = description;
	}

	/// Add a permission to this project role
	public void AddPermission(ProjectPermission permission)
	{
		if (permission == null)
			throw new ArgumentNullException(nameof(permission));

		if (ProjectRolePermissions.Any(prp => prp.ProjectPermissionId == permission.Id))
			return;

		ProjectRolePermissions.Add(new ProjectRolePermission
		{
			ProjectRoleId = this.Id,
			ProjectPermissionId = permission.Id,
			ProjectPermission = permission
		});
	}

	/// Remove a permission from this project role
	public void RemovePermission(ProjectPermission permission)
	{
		if (permission == null)
			throw new ArgumentNullException(nameof(permission));

		ProjectRolePermissions.RemoveAll(prp => prp.ProjectPermissionId == permission.Id);
	}

	/// Clear all permissions
	public void ClearPermissions()
	{
		ProjectRolePermissions.Clear();
	}

	/// Set permissions for this project role
	public void SetPermissions(List<ProjectPermission> permissions)
	{
		if (permissions == null)
			throw new ArgumentNullException(nameof(permissions));

		ProjectRolePermissions.Clear();
		ProjectRolePermissions.AddRange(permissions.Select(p => new ProjectRolePermission
		{
			ProjectRoleId = this.Id,
			ProjectPermissionId = p.Id,
			ProjectPermission = p
		}));
	}

	/// Check if this role has a specific permission
	public bool HasPermission(Guid permissionId)
	{
		return ProjectRolePermissions.Any(prp => prp.ProjectPermissionId == permissionId);
	}

	/// Check if this role has a specific permission by code
	public bool HasPermission(string permissionCode)
	{
		if (string.IsNullOrWhiteSpace(permissionCode))
			return false;

		return ProjectRolePermissions.Any(prp => prp.ProjectPermission.Code == permissionCode);
	}
}
