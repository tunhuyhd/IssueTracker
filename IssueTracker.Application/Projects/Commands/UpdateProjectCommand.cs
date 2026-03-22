using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.Projects;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
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
public class UpdateProjectCommandHandler(IRepository<Project> projectRepository, ICurrentUser currentUser) : IRequestHandler<UpdateProjectCommand, ProjectDetailDto>
{
	public async Task<ProjectDetailDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
	{
		var project = await projectRepository.GetByIdAsync(request.Id, "UserProject",cancellationToken);

		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != ProjectRoleCode.ProjectAdmin)
		{
			throw new UnauthorizedAccessException("You do not have permission to update this project.");
		}

		if (project == null)
		{
			throw new ArgumentNullException(nameof(project));
		}

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
