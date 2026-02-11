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
	public string Password { get; set; } = string.Empty;
}

public class RegisterCommandHandler(IRepository<User> userRepository, IApplicationDbContext dbContext) : IRequestHandler<RegisterCommand, RegisterResponse>
{
	public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		var existingUser = await userRepository.GetOneAsync(u => u.Username == request.Username, cancellationToken);

		var roleUser = await dbContext.Roles.FirstOrDefaultAsync(r => r.Code == "USER", cancellationToken);

		if (existingUser != null)
		{
			throw new InvalidOperationException("Username already exists.");
		}

		var newUser = new User
		{
			Username = request.Username,
			IsActive = true,
			RoleId = roleUser != null ? roleUser.Id : Guid.Empty
		};

		newUser.SetPasswordHash(request.Password);

		await userRepository.AddAsync(newUser, cancellationToken);
		await userRepository.SaveChangesAsync(cancellationToken);

		return new RegisterResponse
		{
			UserId = newUser.Id,
			Username = newUser.Username
		};
	}
}