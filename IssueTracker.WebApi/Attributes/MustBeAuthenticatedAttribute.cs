using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.WebApi.Attributes;

/// <summary>
/// Authorization attribute that only requires user to be authenticated
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MustBeAuthenticatedAttribute : TypeFilterAttribute
{
	public MustBeAuthenticatedAttribute() 
		: base(typeof(AuthenticationFilter))
	{
	}
}
