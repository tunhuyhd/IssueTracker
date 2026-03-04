using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Extensions;
using IssueTracker.Application.Common.Response;
using IssueTracker.Application.Common.Services;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Application.Projects.Commands;

public record CreateProjectCommand : IRequest<CreateProjectResponse>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}


public class CreateProjectCommandHandler(IRepository<Project> projectRepository, ICurrentUser currentUser) : IRequestHandler<CreateProjectCommand, CreateProjectResponse>
{
	public async Task<CreateProjectResponse> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
