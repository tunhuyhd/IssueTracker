using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.ProjectPermission;
using IssueTracker.Application.Common.Dto.ProjectRoles;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Commands;

public class AddProjectPermissionIntoRoleCommand : IRequest<ProjectRoleDto>
{
	public Guid ProjectRoleId { get; set; }
	public Guid ProjectPermissionId { get; set; }

}
public class AddProjectPermissionIntoRoleCommandHandler(ICurrentUser currentUser, IRepository<Domain.Entities.ProjectRole> projectRoleRepository, IRepository<Domain.Entities.ProjectPermission> projectPermissionRepository) : IRequestHandler<AddProjectPermissionIntoRoleCommand, ProjectRoleDto>
{
	public async Task<ProjectRoleDto> Handle(AddProjectPermissionIntoRoleCommand request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only admin can add project permission into role");
		}

		var projectRole = await projectRoleRepository.GetByIdAsync(request.ProjectRoleId, cancellationToken);

		if (projectRole == null)
		{
			throw new KeyNotFoundException($"Project role with ID {request.ProjectRoleId} not found.");
		}

		var projectPermission = await projectPermissionRepository.GetByIdAsync(request.ProjectPermissionId, cancellationToken);

		if (projectPermission == null)
		{
			throw new KeyNotFoundException($"Project permission with ID {request.ProjectPermissionId} not found.");
		}

		projectRole.AddPermission(projectPermission);

		await projectRoleRepository.UpdateAsync(projectRole, cancellationToken);
		await projectRoleRepository.SaveChangesAsync(cancellationToken);

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
