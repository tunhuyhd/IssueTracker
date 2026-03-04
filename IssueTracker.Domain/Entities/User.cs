using IssueTracker.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace IssueTracker.Domain.Entities;

[Table("users")]
public class User : AuditableEntity, IAggregateRoot
{
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

	[Column("salt")]
	public string? Salt { get; set; }

	[Column("is_active")]
    public bool IsActive { get; set; } = true;

	[Column("image_url")]
	public string? ImageUrl { get; set; } = default!;

	[Column("refresh_token")]
	public string? RefreshToken { get; set; } = null!;

	[Column("refresh_token_expiry_time")]
	public DateTime? RefreshTokenExpiryTime { get; set; }

	[Column("role_id")]
	public Guid RoleId { get; set; }
	public Role Role { get; set; }

	public List<UserProject> UserProjects { get; set; } = [];

	public void SetPasswordHash(string password)
	{
		Salt = GenerateSalt();
		PasswordHash = HashPassword(password, Salt);
	}

	private static string HashPassword(string password, string salt)
	{
		// Implement a method to hash the password with the provided salt
		using SHA256 sha256 = SHA256.Create();
		byte[] data = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + salt));
		StringBuilder hash = new StringBuilder();
		for (int i = 0; i < data.Length; i++)
		{
			hash.Append(data[i].ToString("x2"));
		}
		return hash.ToString();
	}

	public bool IsPasswordValid(string password)
	{
		if (Salt == null) return false;
		string hashedPassword = HashPassword(password, Salt);
		return hashedPassword == PasswordHash;
	}

	private static string GenerateSalt()
	{
		// Implement a method to generate a cryptographic salt
		using var rng = RandomNumberGenerator.Create();
		byte[] saltBytes = new byte[16];
		rng.GetBytes(saltBytes);
		return Convert.ToBase64String(saltBytes);
	}
}
