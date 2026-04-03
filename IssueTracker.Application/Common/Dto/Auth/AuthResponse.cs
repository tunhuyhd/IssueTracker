using IssueTracker.Application.Common.Dto.Roles;

namespace IssueTracker.Application.Common.Dto.Auth;

/// Extended token response với thông tin user đầy đủ
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public AuthUserDto User { get; set; } = new();
}

/// User DTO cho authentication response
public class AuthUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageUrlThumbnail { get; set; }
    public string? ImageUrlSmall { get; set; }
    public bool IsActive { get; set; }
    public RoleDto? Role { get; set; }
    public string[] Permissions { get; set; } = Array.Empty<string>();
}
