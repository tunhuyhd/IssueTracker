using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Projects.Commands;

public class ToggleEnableProjectCommand : IRequest<bool>
{
	public Guid Id { get; set; }
}
public class ToggleEnableProjectCommandHandler(IRepository<Project> projectRepository, ICurrentUser currentUser) : IRequestHandler<ToggleEnableProjectCommand, bool>
{
	public async Task<bool> Handle(ToggleEnableProjectCommand request, CancellationToken cancellationToken)
	{
		var currentUserRoles = currentUser.GetRoleCode();

		if (currentUserRoles != ProjectRoleCode.ProjectAdmin)
		{
			throw new InvalidOperationException("You don't have permission to enable this project");
		}

		var project = await projectRepository.GetByIdAsync(request.Id);

		if (project == null)
		{
			throw new ArgumentNullException(nameof(project));
		}

		project.ToggleEnabled();

		await projectRepository.UpdateAsync(project, cancellationToken);
		await projectRepository.SaveChangesAsync(cancellationToken);

		return project.IsEnabled;
	}
}
