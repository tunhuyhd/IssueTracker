using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Attributes;

/// <summary>
/// Authorization attribute that requires user to have specific role(s)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class MustHaveRoleAttribute : TypeFilterAttribute
{
	/// <summary>
	/// Initialize with a single role code
	/// </summary>
	/// <param name="roleCode">The role code required</param>
	public MustHaveRoleAttribute(string roleCode) 
		: base(typeof(RoleAuthorizationFilter))
	{
		Arguments = new object[] { new[] { roleCode } };
	}

	/// <summary>
	/// Initialize with multiple role codes
	/// </summary>
	/// <param name="roleCodes">The role codes required (user must have at least one)</param>
	public MustHaveRoleAttribute(params string[] roleCodes) 
		: base(typeof(RoleAuthorizationFilter))
	{
		Arguments = new object[] { roleCodes };
	}
}
