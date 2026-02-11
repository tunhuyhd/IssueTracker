namespace IssueTracker.Application.Common.Authorization;

public interface ICurrentUser
{
	Guid GetUserId();
	string GetUsername();
	string GetUserEmail();
	string GetFullName();
	string GetRoleCode();
	string GetImageUrl();
	bool IsAuthenticated();
	string[] GetPermissions();
	bool HasPermission(string permission);
}
