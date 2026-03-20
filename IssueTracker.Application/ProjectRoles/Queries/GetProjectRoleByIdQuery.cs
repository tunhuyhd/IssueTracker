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
		var projectRole = await dbContext.ProjectRoles
			.Include(r => r.ProjectRolePermissions.Where(prp => prp.DeletedOn == null && prp.ProjectPermission.DeletedOn == null))
				.ThenInclude(prp => prp.ProjectPermission)
			.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

		if (projectRole == null)
		{
			return null;
		}

		return new ProjectRoleDto
		{
			Id = projectRole.Id,
			Code = projectRole.Code,
			Description = projectRole.Description,
			Permissions = projectRole.ProjectRolePermissions
				.Where(prp => prp.DeletedOn == null && prp.ProjectPermission != null && prp.ProjectPermission.DeletedOn == null)
				.Select(prp => new ProjectPermissionDto
				{
					Id = prp.ProjectPermission.Id,
					Code = prp.ProjectPermission.Code,
					Name = prp.ProjectPermission.Name
				}).ToList()
		};
	}
}
