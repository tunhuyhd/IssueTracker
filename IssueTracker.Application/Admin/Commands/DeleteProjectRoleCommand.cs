using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Commands;

public class DeleteProjectRoleCommand : IRequest
{
	public Guid ProjectRoleId { get; set; }
}
public class DeleteProjectRoleCommandHandler(ICurrentUser currentUser, IRepository<ProjectRole> projectRoleRepository) : IRequestHandler<DeleteProjectRoleCommand>
{
	public async Task Handle(DeleteProjectRoleCommand request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only Admin can delete project roles.");
		}

		var targetProjectRole = await projectRoleRepository.GetByIdAsync(request.ProjectRoleId, "", cancellationToken);

		if (targetProjectRole == null)
		{
			throw new KeyNotFoundException($"Project role with ID {request.ProjectRoleId} not found.");
		}

		await projectRoleRepository.DeleteAsync(targetProjectRole, cancellationToken);
		await projectRoleRepository.SaveChangesAsync(cancellationToken);
	}
}
