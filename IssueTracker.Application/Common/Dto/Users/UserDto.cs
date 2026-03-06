namespace IssueTracker.Application.Common.Dto.Users;

public class UserDto
{
	public Guid Id { get; set; }
	public string Username { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string? FullName { get; set; }
	public string? ImageUrl { get; set; }
	public bool IsActive { get; set; }
	public Guid RoleId { get; set; }
	public string RoleCode { get; set; } = string.Empty;
	public string RoleName { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
}
