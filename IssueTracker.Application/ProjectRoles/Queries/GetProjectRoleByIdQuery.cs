using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Dto.ProjectPermission;
using IssueTracker.Application.Common.Dto.ProjectRoles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.ProjectRoles.Queries;

public class GetProjectRoleByIdQuery : IRequest<ProjectRoleDto>
{
	public Guid Id { get; set; }
}
public class GetProjectRoleByIdQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetProjectRoleByIdQuery, ProjectRoleDto>
{
	public async Task<ProjectRoleDto> Handle(GetProjectRoleByIdQuery request, CancellationToken cancellationToken)
	{
		var projectRole = await dbContext.ProjectRoles.Include(r => r.ProjectPermissions).FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
		
		if (projectRole == null)
		{
			return null;
		}

		return new ProjectRoleDto
		{
			Id = projectRole.Id,
			Code = projectRole.Code,
			Description = projectRole.Description,
			Permissions = projectRole.ProjectPermissions.Select(p => new ProjectPermissionDto
			{
				Id = p.Id,
				Code = p.Code,
				Name = p.Name
			}).ToList()
		};
	}
}
