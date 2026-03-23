using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Projects.Commands;

public class ToggleEnableProjectCommand : IRequest<bool>
{
	public Guid Id { get; set; }
}
public class ToggleEnableProjectCommandHandler(IRepository<Project> projectRepository, ICurrentUser currentUser, IApplicationDbContext dbContext) : IRequestHandler<ToggleEnableProjectCommand, bool>
{
	public async Task<bool> Handle(ToggleEnableProjectCommand request, CancellationToken cancellationToken)
	{
		var currentUserId = currentUser.GetUserId();

		var currentUserProject = await dbContext.UserProjects
			.FirstOrDefaultAsync(rip => rip.UserId == currentUserId &&  rip.ProjectId == request.Id,cancellationToken);

		var projectAdminId = currentUserProject.ProjectRoleId;
		
		var roleInProject = await dbContext.ProjectRoles
			.FirstOrDefaultAsync(pr => pr.Id == projectAdminId, cancellationToken);

		if (roleInProject.Code != ProjectRoleCode.ProjectAdmin)
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
