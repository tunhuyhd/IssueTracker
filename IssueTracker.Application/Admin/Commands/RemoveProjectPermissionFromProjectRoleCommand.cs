using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.ProjectPermission;
using IssueTracker.Application.Common.Dto.ProjectRoles;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Commands;

public class RemoveProjectPermissionFromProjectRoleCommand : IRequest<ProjectRoleDto>
{
	public Guid ProjectRoleId { get; set; }
	public Guid ProjectPermissionId { get; set; }
}
public class RemoveProjectPermissionFromProjectRoleCommandHandler(ICurrentUser currentUser, IApplicationDbContext dbContext) : IRequestHandler<RemoveProjectPermissionFromProjectRoleCommand, ProjectRoleDto>
{
	public async Task<ProjectRoleDto> Handle(RemoveProjectPermissionFromProjectRoleCommand request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only admin can remove project permission from role");
		}

		// Verify ProjectRole exists
		var projectRoleExists = await dbContext.ProjectRoles
			.AnyAsync(pr => pr.Id == request.ProjectRoleId, cancellationToken);

		if (!projectRoleExists)
		{
			throw new KeyNotFoundException($"Project role with ID {request.ProjectRoleId} not found.");
		}

		// Verify ProjectPermission exists
		var projectPermissionExists = await dbContext.ProjectPermissions
			.AnyAsync(pp => pp.Id == request.ProjectPermissionId, cancellationToken);

		if (!projectPermissionExists)
		{
			throw new KeyNotFoundException($"Project permission with ID {request.ProjectPermissionId} not found.");
		}

		// Find and delete the ProjectRolePermission entity directly
		var projectRolePermission = await dbContext.ProjectRolePermissions
			.FirstOrDefaultAsync(prp => 
				prp.ProjectRoleId == request.ProjectRoleId && 
				prp.ProjectPermissionId == request.ProjectPermissionId && prp.DeletedOn == null, 
				cancellationToken);

		if (projectRolePermission != null)
		{
			dbContext.ProjectRolePermissions.Remove(projectRolePermission);
			await dbContext.SaveChangesAsync(cancellationToken);
		}

		// Load ProjectRole with updated permissions to return
		var projectRole = await dbContext.ProjectRoles
			.Include(pr => pr.ProjectRolePermissions.Where(prp => prp.DeletedOn == null && prp.ProjectPermission.DeletedOn == null))
				.ThenInclude(prp => prp.ProjectPermission)
			.FirstOrDefaultAsync(pr => pr.Id == request.ProjectRoleId, cancellationToken);

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
