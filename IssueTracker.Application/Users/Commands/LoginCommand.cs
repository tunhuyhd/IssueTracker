using IssueTracker.Application.Common.Dto;
using IssueTracker.Application.Common.Dto.Users;
using IssueTracker.Application.Identity;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Users.Commands;

public class LoginCommand : IRequest<TokenResponse>
{
	public string Username { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}
public class LoginCommandHandler(IRepository<User> userRepository, ITokenService tokenService) : IRequestHandler<LoginCommand, TokenResponse>
{
	public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		var user = await userRepository.GetOneAsync(u => u.Username == request.Username);
		if (user == null || !user.IsPasswordValid(request.Password))
			throw new Exception("Invalid credentials");

		var token = await tokenService.GenerateTokenAsync(user.Id);
		return token;
	}
}
