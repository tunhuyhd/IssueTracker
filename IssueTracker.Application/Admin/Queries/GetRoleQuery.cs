using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.Roles;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Queries;

public class GetRoleQuery : IRequest<RoleDto>
{
	public Guid Id { get; set; }
}
public class GetRoleQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<GetRoleQuery, RoleDto>
{
	public async Task<RoleDto> Handle(GetRoleQuery request, CancellationToken cancellationToken)
	{
		var currentUserRoles = currentUser.GetRoleCode();

		if (currentUserRoles != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("You do not have permission to access this resource.");
		}

		var role = await dbContext.Roles.FindAsync(new object[] { request.Id }, cancellationToken);

		if (role == null)
		{
			throw new KeyNotFoundException($"Role with ID '{request.Id}' not found.");
		}

		return new RoleDto
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
		};
	}
}
