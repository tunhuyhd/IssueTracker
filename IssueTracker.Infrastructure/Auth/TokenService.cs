using IssueTracker.Application.Common.Dto;
using IssueTracker.Application.Identity;
using IssueTracker.Domain.Entities;
using IssueTracker.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IssueTracker.Infrastructure.Auth;

public class TokenService : ITokenService
{
	private readonly ApplicationDbContext _dbContext;
	private readonly JwtSettings _jwtSettings;

	public TokenService(ApplicationDbContext dbContext, IOptions<JwtSettings> jwtSettings)
	{
		_dbContext = dbContext;
		_jwtSettings = jwtSettings.Value;
	}

	public async Task<TokenResponse> GenerateTokenAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var user = await _dbContext.Users
			.Include(u => u.Role)
			.ThenInclude(r => r.Permissions)
			.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

		if (user == null)
			throw new Exception("User not found or inactive");

		var accessToken = GenerateAccessToken(user);
		var refreshToken = GenerateRefreshToken();

		// Cập nhật refresh token vào database
		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

		await _dbContext.SaveChangesAsync(cancellationToken);

		return new TokenResponse
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken,
			ExpiresIn = _jwtSettings.ExpirationInMinutes * 60,
			TokenType = "Bearer"
		};
	}

	public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
	{
		var user = await _dbContext.Users
			.Include(u => u.Role)
			.ThenInclude(r => r.Permissions)
			.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken &&
									u.IsActive &&
									u.RefreshTokenExpiryTime > DateTime.UtcNow,
									cancellationToken);

		if (user == null)
			throw new Exception("Invalid or expired refresh token");

		return await GenerateTokenAsync(user.Id, cancellationToken);
	}

	public async Task RevokeTokenAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
		if (user != null)
		{
			user.RefreshToken = null;
			user.RefreshTokenExpiryTime = null;
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
	}

	private string GenerateAccessToken(User user)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new(ClaimTypes.Name, user.Username),
			new(ClaimTypes.Email, user.Email),
			new("full_name", user.FullName ?? string.Empty),
			new("role_code", user.Role?.Code ?? string.Empty),
			new("image_url", user.ImageUrl ?? string.Empty)
		};

		// Thêm permissions vào claims
		if (user.Role?.Permissions?.Any() == true)
		{
			var permissions = user.Role.Permissions.Select(p => p.Code).ToArray();
			claims.Add(new Claim("permissions", JsonSerializer.Serialize(permissions)));
		}

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _jwtSettings.Issuer,
			audience: _jwtSettings.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	private static string GenerateRefreshToken()
	{
		var randomNumber = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomNumber);
		return Convert.ToBase64String(randomNumber);
	}
}
