using IssueTracker.Application.Common.Dto;
using IssueTracker.Application.Common.Dto.Users;
using IssueTracker.Application.Common.Exceptions;
using IssueTracker.Application.Identity;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Users.Commands;

public class LoginCommand : IRequest<TokenResponse>
{
	public string Username { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}
public class LoginCommandHandler(
	IRepository<User> userRepository,
	ITokenService tokenService,
	ILogger<LoginCommandHandler> logger
) : IRequestHandler<LoginCommand, TokenResponse> {

	public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		var user = await userRepository.GetOneAsync(u => u.Username == request.Username);

		if (user == null)
		{
			logger.LogWarning("Login failed: Username '{Username}' not found", request.Username);
			throw new UnauthorizedException("Invalid username or password");
		}

		if (!user.IsPasswordValid(request.Password))
		{
			logger.LogWarning("Login failed: Invalid password for username '{Username}'", request.Username);
			throw new UnauthorizedException("Invalid username or password");
		}

		logger.LogInformation("User '{Username}' logged in successfully", request.Username);
		var token = await tokenService.GenerateTokenAsync(user.Id, cancellationToken);
		return token;
	}
}
