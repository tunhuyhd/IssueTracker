using IssueTracker.Application.Common.Dto.ProjectPermission;
using IssueTracker.Application.Common.Dto.ProjectRoles;
using IssueTracker.Domain.Common;
using MediatR;
using IssueTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Entities.Enum;
using System.Text.Json.Serialization;

namespace IssueTracker.Application.Admin.Commands;

public class UpdateProjectRoleCommand : IRequest<ProjectRoleDto>
{
	[JsonIgnore]
	public Guid Id { get; set; }

	public string? Code { get; set; }
	public string? Description { get; set; }
}

public class UpdateProjectRoleCommandHandler(
	IRepository<Domain.Entities.ProjectRole> repository,
	ICurrentUser currentUser) : IRequestHandler<UpdateProjectRoleCommand, ProjectRoleDto>
{
	public async Task<ProjectRoleDto> Handle(UpdateProjectRoleCommand request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only admins can update project roles.");
		}

     Domain.Entities.ProjectRole projectRole;
		try
		{
			projectRole = await repository.GetByIdAsync(request.Id, "", cancellationToken);
		}
		catch (ArgumentException)
		{
			throw new KeyNotFoundException($"Project role with ID {request.Id} not found.");
		}

		// Update basic info
		projectRole.Update(request.Code, request.Description);

		await repository.UpdateAsync(projectRole, cancellationToken);
		await repository.SaveChangesAsync(cancellationToken);

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
