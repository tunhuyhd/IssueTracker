using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.Projects;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace IssueTracker.Application.Projects.Commands;

public class UpdateProjectCommand : IRequest<ProjectDetailDto>
{
	[JsonIgnore]
	public Guid Id { get; set; }
	public string? Name { get; set; } = string.Empty;
	public string? Description { get; set; } = string.Empty;
}
public class UpdateProjectCommandHandler(IRepository<Project> projectRepository, ICurrentUser currentUser, IApplicationDbContext dbContext) : IRequestHandler<UpdateProjectCommand, ProjectDetailDto>
{
	public async Task<ProjectDetailDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
	{
		var currentUserId = currentUser.GetUserId();

		var userProjectInfo = await dbContext.UserProjects
			.Where(up => up.UserId == currentUserId && up.ProjectId == request.Id)
			.Select(up => new
			{
				up.ProjectRole.Code
			})
			.FirstOrDefaultAsync(cancellationToken);

		if (userProjectInfo == null || userProjectInfo.Code != ProjectRoleCode.ProjectAdmin)
		{
			throw new InvalidOperationException("You don't have permission to enable this project");
		}

		var project = await dbContext.Projects.Include(p => p.UserProjects).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

		project.Update(request.Name, request.Description);

		await projectRepository.UpdateAsync(project, cancellationToken);
		await projectRepository.SaveChangesAsync(cancellationToken);

		return new ProjectDetailDto
		{
			Id = project.Id,
			Name = project.Name,
			Description = project.Description,
			IsEnabled = project.IsEnabled
		};
	}
}
