using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Attributes;

/// <summary>
/// Authorization attribute that requires user to have specific permission(s) in a project
/// The projectId must be provided in route parameters
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MustHaveProjectPermissionAttribute : TypeFilterAttribute
{
	/// <summary>
	/// Initialize with a single project permission code
	/// </summary>
	/// <param name="permissionCode">The project permission code required</param>
	/// <param name="projectIdParameterName">The name of the route parameter containing projectId (default: "projectId")</param>
	public MustHaveProjectPermissionAttribute(
		string permissionCode,
		string projectIdParameterName = "projectId") 
		: base(typeof(ProjectPermissionAuthorizationFilter))
	{
		Arguments = new object[] { new[] { permissionCode }, false, projectIdParameterName };
	}

	/// <summary>
	/// Initialize with multiple project permission codes
	/// </summary>
	/// <param name="permissionCodes">The project permission codes required</param>
	public MustHaveProjectPermissionAttribute(params string[] permissionCodes) 
		: base(typeof(ProjectPermissionAuthorizationFilter))
	{
		Arguments = new object[] { permissionCodes, false, "projectId" };
	}

	/// <summary>
	/// Whether all permissions are required (true) or just one (false)
	/// </summary>
	public bool RequireAll { get; set; }

	/// <summary>
	/// The name of the route parameter containing projectId (default: "projectId")
	/// </summary>
	public string ProjectIdParameterName { get; set; } = "projectId";
}
