using IssueTracker.Application.Common.Dto;
using IssueTracker.Application.Identity;
using MediatR;

namespace IssueTracker.Application.Users.Commands;

public class RefreshTokenCommand : IRequest<TokenResponse>
{
	public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandHandler(ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
	public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
	{
		var token = await tokenService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
		return token;
	}
}
