using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.Users;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Projects.Queries;

public class GetMemberInProjectQuery : IRequest<List<UserInProjectDto>>
{
	public Guid ProjectId { get; set; }
}
public class GetMemberInProjectQueryHandler(IApplicationDbContext context, ICurrentUser currentUser) : IRequestHandler<GetMemberInProjectQuery, List<UserInProjectDto>>
{
	public async Task<List<UserInProjectDto>> Handle(GetMemberInProjectQuery request, CancellationToken cancellationToken)
	{
		var isMember = await context.UserProjects
			.AnyAsync(up => up.ProjectId == request.ProjectId && up.UserId == currentUser.GetUserId(), cancellationToken);

		var roleOfCurrentUser = await context.UserProjects
			.Where(up => up.ProjectId == request.ProjectId && up.UserId == currentUser.GetUserId())
			.Select(up => up.ProjectRole.Code)
			.FirstOrDefaultAsync(cancellationToken);

			if (!isMember && (roleOfCurrentUser != ProjectRoleCode.ProjectAdmin || roleOfCurrentUser != ProjectRoleCode.ProjectManager))
			{
				throw new Exception("You don't have permission to view members in this project.");
			}

			var members = await context.UserProjects
			.Where(up => up.ProjectId == request.ProjectId)
			.Select(up => new UserInProjectDto
			{
				Id = up.UserId,
				Name = up.User.FullName,
				Email = up.User.Email,
				ProjectRoleCode = up.ProjectRole.Code
			})
			.ToListAsync(cancellationToken);
		return members;
	}
}