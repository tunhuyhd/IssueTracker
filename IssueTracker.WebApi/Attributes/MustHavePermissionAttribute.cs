using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Attributes;

/// <summary>
/// Authorization attribute that requires user to have specific permission(s)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class MustHavePermissionAttribute : TypeFilterAttribute
{
	/// <summary>
	/// Initialize with a single permission code
	/// </summary>
	/// <param name="permissionCode">The permission code required</param>
	public MustHavePermissionAttribute(string permissionCode) 
		: base(typeof(PermissionAuthorizationFilter))
	{
		Arguments = new object[] { new[] { permissionCode }, RequireAll = false };
	}

	/// <summary>
	/// Initialize with multiple permission codes
	/// </summary>
	/// <param name="permissionCodes">The permission codes required</param>
	public MustHavePermissionAttribute(params string[] permissionCodes) 
		: base(typeof(PermissionAuthorizationFilter))
	{
		Arguments = new object[] { permissionCodes, RequireAll = false };
	}

	/// <summary>
	/// Whether all permissions are required (true) or just one (false)
	/// </summary>
	public bool RequireAll { get; set; }
}
