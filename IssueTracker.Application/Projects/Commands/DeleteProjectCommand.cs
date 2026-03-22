using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Projects.Commands;

public class DeleteProjectCommand : IRequest
{
	public Guid Id { get; set; }
}
public class DeleteProjectCommandHandler(IRepository<Project> projectRepository, ICurrentUser currentUser) : IRequestHandler<DeleteProjectCommand>
{
	public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != ProjectRoleCode.ProjectAdmin)
		{
			throw new UnauthorizedAccessException("Only admins can delete projects.");
		}

		var targetProject = await projectRepository.GetByIdAsync(request.Id);

		if (targetProject == null)
		{
			throw new Exception("Project not found");
		}

		await projectRepository.DeleteAsync(targetProject);
		await projectRepository.SaveChangesAsync();
	}
}
