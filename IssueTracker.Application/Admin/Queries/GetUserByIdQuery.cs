using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.Users;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Queries;

public class GetUserByIdQuery : IRequest<UserDto>
{
	public Guid UserId { get; set; }
}
public class GetUserByIdQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<GetUserByIdQuery, UserDto>
{
	public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only Admin users can access this resource.");
		}

		var user = await dbContext.Users
			.Include(u => u.Role)
			.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

		if (user == null)
		{
			throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
		}
		return new UserDto
		{
			Id = user.Id,
			Username = user.Username,
			FullName = user.FullName,
			Email = user.Email,
			ImageUrl = user.ImageUrl,
			RoleId = user.RoleId,
			RoleCode = user.Role.Code,
			RoleName = user.Role.Description,
			IsActive = user.IsActive,
			CreatedAt = user.CreatedOn
		};
	}
}
