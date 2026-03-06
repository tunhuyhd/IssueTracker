using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Commands;

public class DeleteProjectPermissionCommand : IRequest<Unit>
{
	public Guid Id { get; set; }
}
public class DeleteProjectPermissionCommandHandler(IRepository<ProjectPermission> repository, ICurrentUser currentUser) : IRequestHandler<DeleteProjectPermissionCommand, Unit>
{
	public async Task<Unit> Handle(DeleteProjectPermissionCommand request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only admins can delete project permissions.");
		}
		var projectPermission = await repository.GetByIdAsync(request.Id, cancellationToken);

		if (projectPermission == null)
		{
			throw new KeyNotFoundException($"Project permission with ID {request.Id} not found.");
		}
		await repository.DeleteAsync(projectPermission, cancellationToken);
		await repository.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}
