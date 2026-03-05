using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Entities.Enum;

namespace IssueTracker.Application.Common.Extensions;

/// <summary>
/// Extension methods for permission checking in business logic
/// </summary>
public static class PermissionExtensions
{
	/// <summary>
	/// Check if current user has specific permission
	/// </summary>
	public static bool HasPermission(this ICurrentUser currentUser, string permissionCode)
	{
		return currentUser.GetPermissions().Contains(permissionCode);
	}

	/// <summary>
	/// Check if current user has any of the specified permissions
	/// </summary>
	public static bool HasAnyPermission(this ICurrentUser currentUser, params string[] permissionCodes)
	{
		var userPermissions = currentUser.GetPermissions();
		return permissionCodes.Any(p => userPermissions.Contains(p));
	}

	/// <summary>
	/// Check if current user has all of the specified permissions
	/// </summary>
	public static bool HasAllPermissions(this ICurrentUser currentUser, params string[] permissionCodes)
	{
		var userPermissions = currentUser.GetPermissions();
		return permissionCodes.All(p => userPermissions.Contains(p));
	}

	/// <summary>
	/// Throw exception if user doesn't have permission
	/// </summary>
	public static void EnsureHasPermission(this ICurrentUser currentUser, string permissionCode)
	{
		if (!currentUser.HasPermission(permissionCode))
		{
			throw new UnauthorizedAccessException($"User does not have required permission: {permissionCode}");
		}
	}

	/// <summary>
	/// Throw exception if user doesn't have any of the specified permissions
	/// </summary>
	public static void EnsureHasAnyPermission(this ICurrentUser currentUser, params string[] permissionCodes)
	{
		if (!currentUser.HasAnyPermission(permissionCodes))
		{
			var required = string.Join(", ", permissionCodes);
			throw new UnauthorizedAccessException($"User does not have any of required permissions: {required}");
		}
	}

	/// <summary>
	/// Throw exception if user doesn't have all of the specified permissions
	/// </summary>
	public static void EnsureHasAllPermissions(this ICurrentUser currentUser, params string[] permissionCodes)
	{
		if (!currentUser.HasAllPermissions(permissionCodes))
		{
			var required = string.Join(", ", permissionCodes);
			throw new UnauthorizedAccessException($"User does not have all required permissions: {required}");
		}
	}

	/// <summary>
	/// Check if current user has specific role
	/// </summary>
	public static bool HasRole(this ICurrentUser currentUser, string roleCode)
	{
		return currentUser.GetRoleCode() == roleCode;
	}

	/// <summary>
	/// Check if current user has any of the specified roles
	/// </summary>
	public static bool HasAnyRole(this ICurrentUser currentUser, params string[] roleCodes)
	{
		var userRole = currentUser.GetRoleCode();
		return roleCodes.Contains(userRole);
	}

	/// <summary>
	/// Throw exception if user doesn't have role
	/// </summary>
	public static void EnsureHasRole(this ICurrentUser currentUser, string roleCode)
	{
		if (!currentUser.HasRole(roleCode))
		{
			throw new UnauthorizedAccessException($"User does not have required role: {roleCode}");
		}
	}

	/// <summary>
	/// Check if current user is Admin
	/// </summary>
	public static bool IsAdmin(this ICurrentUser currentUser)
	{
		return currentUser.GetRoleCode() == RoleCode.Admin;
	}

	/// <summary>
	/// Throw exception if user is not Admin
	/// </summary>
	public static void EnsureIsAdmin(this ICurrentUser currentUser)
	{
		if (!currentUser.IsAdmin())
		{
			throw new UnauthorizedAccessException("User must be an administrator");
		}
	}

	/// <summary>
	/// Check if user can manage other users
	/// </summary>
	public static bool CanManageUsers(this ICurrentUser currentUser)
	{
		return currentUser.HasPermission(PermissionCode.UserManage);
	}

	/// <summary>
	/// Check if user can view all projects
	/// </summary>
	public static bool CanViewAllProjects(this ICurrentUser currentUser)
	{
		return currentUser.HasPermission(PermissionCode.ViewAllProjects);
	}

	/// <summary>
	/// Check if user can modify system settings
	/// </summary>
	public static bool CanModifySystemSettings(this ICurrentUser currentUser)
	{
		return currentUser.HasPermission(PermissionCode.SystemSettings);
	}
}
