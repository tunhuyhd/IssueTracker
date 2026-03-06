using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Dto.ProjectPermission;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.ProjectRoles.Queries;

public class GetListProjetPermissionQuerry : IRequest<List<ProjectPermissionDto>>
{
}

public class GetListProjetPermissionQuerryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetListProjetPermissionQuerry, List<ProjectPermissionDto>>
{
	public async Task<List<ProjectPermissionDto>> Handle(GetListProjetPermissionQuerry request, CancellationToken cancellationToken)
	{
		var permissions = await dbContext.ProjectPermissions.ToListAsync(cancellationToken);

		return permissions.Select(p => new ProjectPermissionDto
		{
			Id = p.Id,
			Code = p.Code,
			Name = p.Name
		}).ToList();
	}
}