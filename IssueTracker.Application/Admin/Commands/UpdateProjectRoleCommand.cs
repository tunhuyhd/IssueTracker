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

	public string? Code { get; set; } = string.Empty;
	public string? Description { get; set; } = string.Empty;
	public List<Guid>? PermissionIds { get; set; }
}

public class UpdateProjectRoleCommandHandler(
	IRepository<Domain.Entities.ProjectRole> repository,
	IRepository<ProjectPermission> permissionRepository,
	ICurrentUser currentUser) : IRequestHandler<UpdateProjectRoleCommand, ProjectRoleDto>
{
	public async Task<ProjectRoleDto> Handle(UpdateProjectRoleCommand request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only admins can update project roles.");
		}

		var projectRole = await repository.GetByIdAsync(request.Id, cancellationToken);

		if (projectRole == null)
		{
			throw new KeyNotFoundException($"Project role with ID {request.Id} not found.");
		}

		// Update basic info
		projectRole.Update(request.Code, request.Description);

		// Update permissions if provided
		if (request.PermissionIds != null && request.PermissionIds.Any())
		{
			var permissions = await permissionRepository.ListAsync(
				p => request.PermissionIds.Contains(p.Id),
				cancellationToken);

			if (permissions.Count != request.PermissionIds.Count)
			{
				throw new InvalidOperationException("One or more permission IDs are invalid.");
			}

			projectRole.SetPermissions(permissions);
		}

		await repository.UpdateAsync(projectRole, cancellationToken);
		await repository.SaveChangesAsync(cancellationToken);

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
