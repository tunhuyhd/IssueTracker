using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Extensions;
using IssueTracker.Application.Common.Response;
using IssueTracker.Application.Common.Services;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Application.Projects.Commands;

public record CreateProjectCommand : IRequest<CreateProjectResponse>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}


public class CreateProjectCommandHandler(
	IRepository<Project> projectRepository,
	ICurrentUser currentUser,
	IRepository<UserProject> userProjectRepository,
	IApplicationDbContext dbContext) : IRequestHandler<CreateProjectCommand, CreateProjectResponse>
{
	public async Task<CreateProjectResponse> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
	{
		var currentUserId = currentUser.GetUserId();

		var projectRoleId = await dbContext.ProjectRoles
			.Where(pr => pr.Code == ProjectRoleCode.ProjectAdmin)
			.Select(pr => pr.Id)
			.FirstOrDefaultAsync(cancellationToken);

		var project = new Project
		{
			Name = request.Name,
			Description = request.Description,
			OwnerId = currentUserId
		};

		await projectRepository.AddAsync(project);
		await projectRepository.SaveChangesAsync(cancellationToken);

		var userProject = new UserProject
		{
			UserId = currentUserId,
			ProjectId = project.Id,
			ProjectRoleId = projectRoleId,
		};

		await userProjectRepository.AddAsync(userProject);
		await userProjectRepository.SaveChangesAsync(cancellationToken);

		return new CreateProjectResponse
		{
			Id = project.Id,
			Name = project.Name,
			Description = project.Description,
			OwnerId = project.OwnerId.ToString(),
			OwnerName = currentUser.GetUsername(),
		};
	}
}
