using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Projects.Queries;

public class GetCurrentUserRoleInProjectQuery : IRequest<UserInProjectDto>
{
	public Guid ProjectId { get; set; }

}
public class GetCurrentUserRoleInProjectQueryHandler(IApplicationDbContext context, ICurrentUser currentUser) : IRequestHandler<GetCurrentUserRoleInProjectQuery, UserInProjectDto>
{
	public async Task<UserInProjectDto> Handle(GetCurrentUserRoleInProjectQuery request, CancellationToken cancellationToken)
	{
		var userId = currentUser.GetUserId();

		var userProject = await context.UserProjects
			.FirstOrDefaultAsync(up => up.ProjectId == request.ProjectId && up.UserId == userId, cancellationToken);

		var projectRoleCode = userProject?.ProjectRole.Code ?? "NoRole";

		return new UserInProjectDto
		{
			Id = userId,
			Name = currentUser.GetFullName(),
			ProjectRoleCode = projectRoleCode,
		};
	}
}
