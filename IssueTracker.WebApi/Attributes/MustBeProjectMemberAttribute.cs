using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Attributes;

/// <summary>
/// Authorization attribute that requires user to be a member of a project
/// The projectId must be provided in route parameters
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MustBeProjectMemberAttribute : TypeFilterAttribute
{
	/// <summary>
	/// Initialize with default projectId parameter name
	/// </summary>
	/// <param name="projectIdParameterName">The name of the route parameter containing projectId (default: "projectId")</param>
	public MustBeProjectMemberAttribute(string projectIdParameterName = "projectId") 
		: base(typeof(ProjectMemberAuthorizationFilter))
	{
		Arguments = new object[] { projectIdParameterName };
	}
}
