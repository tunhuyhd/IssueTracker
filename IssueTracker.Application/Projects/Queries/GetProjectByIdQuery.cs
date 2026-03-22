using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Dto.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Projects.Queries;

public class GetProjectByIdQuery : IRequest<ProjectDetailDto>
{
	public Guid Id { get; set; }
}
public class GetProjectByIdQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetProjectByIdQuery, ProjectDetailDto>
{
	public async Task<ProjectDetailDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
	{
		var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);


		if (project == null)
			throw new Exception("Project not found");

		return new ProjectDetailDto
		{
			Id = project.Id,
			Name = project.Name,
			Description = project.Description,
			IsEnabled = project.IsEnabled
		};
	}
}
