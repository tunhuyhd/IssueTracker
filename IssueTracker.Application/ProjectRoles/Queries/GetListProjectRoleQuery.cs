using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.ProjectPermission;
using IssueTracker.Application.Common.Dto.ProjectRoles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.ProjectRoles.Queries;

public class GetListProjectRoleQuery : IRequest<List<ProjectRoleDto>>
{
}
public class GetListProjectRoleQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetListProjectRoleQuery, List<ProjectRoleDto>>
{
	public async Task<List<ProjectRoleDto>> Handle(GetListProjectRoleQuery request, CancellationToken cancellationToken)
	{
		var projectRoles = await dbContext.ProjectRoles.ToListAsync();

		return projectRoles.Select(r => new ProjectRoleDto
		{
			Id = r.Id,
			Code = r.Code,
			Description = r.Description,
			Permissions = r.ProjectPermissions.Select(p => new ProjectPermissionDto
			{
				Id = p.Id,
				Code = p.Code,
				Name = p.Name
			}).ToList()
		}).ToList();
	}
}
