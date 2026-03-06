using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.Roles;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Queries;

public class GetListPermissionQuery : IRequest<List<PermissionDto>>
{
}
public class GetListPermissionQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<GetListPermissionQuery, List<PermissionDto>>
{
	public async Task<List<PermissionDto>> Handle(GetListPermissionQuery request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only admin can access this resource.");
		}

		var permissions = await dbContext.Permissions.ToListAsync(cancellationToken);
		return permissions.Select(p => new PermissionDto
		{
			Id = p.Id,
			Code = p.Code,
			Name = p.Name
		}).ToList();
	}
}