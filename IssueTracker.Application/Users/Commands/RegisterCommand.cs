using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Dto.User;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Users.Commands;

public class RegisterCommand : IRequest<RegisterResponse>
{
	public string Username { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}

public class RegisterCommandHandler(IRepository<User> userRepository, IApplicationDbContext dbContext) : IRequestHandler<RegisterCommand, RegisterResponse>
{
	public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		// Check for duplicate username or email
		var existingUser = await userRepository.GetOneAsync(
			u => u.Username == request.Username || u.Email == request.Email, 
			cancellationToken);

		if (existingUser != null)
		{
			if (existingUser.Username == request.Username)
			{
				throw new InvalidOperationException("Username already exists.");
			}
			throw new InvalidOperationException("Email already exists.");
		}

		var roleUser = await dbContext.Roles.FirstOrDefaultAsync(r => r.Code == "USER", cancellationToken);

		if (roleUser == null)
		{
			throw new InvalidOperationException("USER role not found. Please seed the database.");
		}

		var newUser = new User
		{
			Username = request.Username,
			Email = request.Email,
			IsActive = true,
			RoleId = roleUser.Id
		};

		newUser.SetPasswordHash(request.Password);

		await userRepository.AddAsync(newUser, cancellationToken);
		await dbContext.SaveChangesAsync(cancellationToken);

		return new RegisterResponse
		{
			UserId = newUser.Id,
			Username = newUser.Username
		};
	}
}