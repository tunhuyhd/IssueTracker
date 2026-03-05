using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.Roles;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Queries;

public class GetListRoleQuery : IRequest<List<RoleDto>>
{

}
public class GetListRoleQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<GetListRoleQuery, List<RoleDto>>
{
	public async Task<List<RoleDto>> Handle(GetListRoleQuery request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only admin can access this resource.");
		}

		var roles = await dbContext.Roles.Include(r => r.Permissions).ToListAsync(cancellationToken);
		
		var roleDtos = roles.Select(role => new RoleDto
		{
			Id = role.Id,
			Code = role.Code,
			Description = role.Description,
			Permissions = role.Permissions.Select(permission => new PermissionDto
			{
				Id = permission.Id,
				Code = permission.Code,
				Name = permission.Name
			}).ToList()
		}).ToList();

		return roleDtos;
	}
}
