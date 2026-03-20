using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.ProjectPermission;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Commands;

public class AddProjectPermissionCommand : IRequest<ProjectPermissionDto>
{
	public string Name { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
}
public class AddProjectPermissionCommandHandler(
	IRepository<ProjectPermission> repository, 
	IRepository<ProjectRole> projectRoleRepository,
	ICurrentUser currentUser) : IRequestHandler<AddProjectPermissionCommand, ProjectPermissionDto>
{
	public async Task<ProjectPermissionDto> Handle(AddProjectPermissionCommand request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only admins can add project permissions.");
		}

		var existingPermission = await repository.GetOneAsync(p => p.Code == request.Code, cancellationToken);

		if (existingPermission != null)
		{
			throw new InvalidOperationException($"A project permission with code '{request.Code}' already exists.");
		}

		var newPermission = new ProjectPermission
		{
			Name = request.Name,
			Code = request.Code
		};

		await repository.AddAsync(newPermission);
		await repository.SaveChangesAsync(cancellationToken);

		return new ProjectPermissionDto
		{
			Id = newPermission.Id,
			Name = newPermission.Name,
			Code = newPermission.Code
		};
	}
}
