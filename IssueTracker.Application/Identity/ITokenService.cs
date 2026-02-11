using IssueTracker.Application.Common.Dto;
using IssueTracker.Application.Common.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Identity;

public interface ITokenService
{
	Task<TokenResponse> GenerateTokenAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
	Task RevokeTokenAsync(Guid userId, CancellationToken cancellationToken = default);
}
