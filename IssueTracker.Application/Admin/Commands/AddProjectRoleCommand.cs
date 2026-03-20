using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.ProjectRoles;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Commands;

public class AddProjectRoleCommand : IRequest<ProjectRoleDto>
{
	public string Code { get; set; }
	public string Description { get; set; }
}
public class AddProjectRoleCommandHandler(IRepository<Domain.Entities.ProjectRole> projectRoleRepository, ICurrentUser currentUser) : IRequestHandler<AddProjectRoleCommand, ProjectRoleDto>
{
	public async Task<ProjectRoleDto> Handle(AddProjectRoleCommand request, CancellationToken cancellationToken)
	{
		var currentUserRoles = currentUser.GetRoleCode();

		if (currentUserRoles != RoleCode.Admin) {
			throw new UnauthorizedAccessException("Only Admin can add project roles.");
		}

		var existingRole = await projectRoleRepository.GetOneAsync(r => r.Code == request.Code && r.DeletedOn == null);

		if (existingRole != null) {
			throw new InvalidOperationException($"A project role with code '{request.Code}' already exists.");
		}

		var projectRole = new Domain.Entities.ProjectRole
		{
			Code = request.Code,
			Description = request.Description
		};
		
		projectRoleRepository.AddAsync(projectRole);
		projectRoleRepository.SaveChangesAsync();

		return new ProjectRoleDto
		{
			Id = projectRole.Id,
			Code = projectRole.Code,
			Description = projectRole.Description
		};
	}
}
