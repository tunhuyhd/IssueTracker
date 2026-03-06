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

	public List<ProjectPermission> ProjectPermissions { get; set; } = [];

	/// Update project role information
	public void Update(string code, string description)
	{
		if (string.IsNullOrWhiteSpace(code))
			throw new ArgumentException("Project role code cannot be empty", nameof(code));

		if (string.IsNullOrWhiteSpace(description))
			throw new ArgumentException("Project role description cannot be empty", nameof(description));

		Code = code;
		Description = description;
	}

	/// Add a permission to this project role
	public void AddPermission(ProjectPermission permission)
	{
		if (permission == null)
			throw new ArgumentNullException(nameof(permission));

		if (ProjectPermissions.Any(p => p.Id == permission.Id))
			return; // Already exists

		ProjectPermissions.Add(permission);
	}

	/// Remove a permission from this project role
	public void RemovePermission(ProjectPermission permission)
	{
		if (permission == null)
			throw new ArgumentNullException(nameof(permission));

		ProjectPermissions.RemoveAll(p => p.Id == permission.Id);
	}

	/// Clear all permissions
	public void ClearPermissions()
	{
		ProjectPermissions.Clear();
	}

	/// Set permissions for this project role
	public void SetPermissions(List<ProjectPermission> permissions)
	{
		if (permissions == null)
			throw new ArgumentNullException(nameof(permissions));

		ProjectPermissions.Clear();
		ProjectPermissions.AddRange(permissions);
	}

	/// Check if this role has a specific permission
	public bool HasPermission(Guid permissionId)
	{
		return ProjectPermissions.Any(p => p.Id == permissionId);
	}

	/// Check if this role has a specific permission by code
	public bool HasPermission(string permissionCode)
	{
		if (string.IsNullOrWhiteSpace(permissionCode))
			return false;

		return ProjectPermissions.Any(p => p.Code == permissionCode);
	}
}
