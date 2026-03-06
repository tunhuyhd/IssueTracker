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

public class GetListUserQuery : IRequest<List<UserDto>>
{

}
public class GetListUserQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<GetListUserQuery, List<UserDto>>
{
	public async Task<List<UserDto>> Handle(GetListUserQuery request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only Admin can access this resource.");
		}

		var users = await dbContext.Users.Include(u => u.Role).ToListAsync(cancellationToken);

		return users.Select(u => new UserDto
		{
			Id = u.Id,
			FullName = u.FullName,
			Email = u.Email,
			RoleCode = u.Role.Code,
			IsActive = u.IsActive,
		}).ToList();
	}
}