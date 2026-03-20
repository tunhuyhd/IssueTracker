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
		var projectRoles = await dbContext.ProjectRoles
			.Include(r => r.ProjectRolePermissions.Where(prp => prp.DeletedOn == null && prp.ProjectPermission.DeletedOn == null))
				.ThenInclude(prp => prp.ProjectPermission)
			.Where(r => r.DeletedOn == null)
			.ToListAsync();

		return projectRoles.Select(r => new ProjectRoleDto
		{
			Id = r.Id,
			Code = r.Code,
			Description = r.Description,
			Permissions = r.ProjectRolePermissions
				.Where(prp => prp.DeletedOn == null && prp.ProjectPermission != null && prp.ProjectPermission.DeletedOn == null)
				.Select(prp => new ProjectPermissionDto
				{
					Id = prp.ProjectPermission.Id,
					Code = prp.ProjectPermission.Code,
					Name = prp.ProjectPermission.Name
				}).ToList()
		}).ToList();
	}
}
