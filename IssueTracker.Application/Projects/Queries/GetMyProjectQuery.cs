using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IssueTracker.Domain.Entities.Enum;

namespace IssueTracker.Application.Projects.Queries;

public class GetMyProjectQuery : IRequest<List<ProjectDto>>
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    /// If true, only return projects where current user has ProjectAdmin role
    public bool IsProjectAdminOnly { get; set; } = false;
}
public class GetMyProjectQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<GetMyProjectQuery, List<ProjectDto>>
{
    public async Task<List<ProjectDto>> Handle(GetMyProjectQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.GetUserId();

        if (currentUserId == Guid.Empty)
            return new List<ProjectDto>();

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var query = dbContext.UserProjects
            .AsNoTracking()
            .Where(pu => pu.UserId == currentUserId && pu.DeletedOn == null)
            .Include(pu => pu.Project)
            .Include(pu => pu.ProjectRole)
            .AsQueryable();

        if (request.IsProjectAdminOnly)
        {
            query = query.Where(pu => pu.ProjectRole.Code == ProjectRoleCode.ProjectAdmin);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(pu => pu.Project.Name.Contains(search));
        }

        var projects = await query
            .Select(pu => pu.Project)
            .Where(p => p.DeletedOn == null)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsEnabled = p.IsEnabled
            })
            .ToListAsync(cancellationToken);

        return projects;
    }
}
