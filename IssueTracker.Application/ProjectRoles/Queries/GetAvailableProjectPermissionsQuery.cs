using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Dto.ProjectPermission;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Application.ProjectRoles.Queries;

public class GetAvailableProjectPermissionsQuery : IRequest<List<ProjectPermissionDto>>
{
	public Guid ProjectRoleId { get; set; }
}

public class GetAvailableProjectPermissionsQueryHandler(IApplicationDbContext dbContext) 
	: IRequestHandler<GetAvailableProjectPermissionsQuery, List<ProjectPermissionDto>>
{
	public async Task<List<ProjectPermissionDto>> Handle(
		GetAvailableProjectPermissionsQuery request, 
		CancellationToken cancellationToken)
	{
		var projectRole = await dbContext.ProjectRoles
			.Include(pr => pr.ProjectRolePermissions.Where(prp => prp.DeletedOn == null && prp.ProjectPermission.DeletedOn == null))
				.ThenInclude(prp => prp.ProjectPermission)
			.FirstOrDefaultAsync(pr => pr.Id == request.ProjectRoleId && pr.DeletedOn == null, cancellationToken);

		if (projectRole == null)
		{
			throw new KeyNotFoundException($"Project role with ID {request.ProjectRoleId} not found.");
		}

		// Only get permission IDs that are not soft deleted (both junction and permission)
		var assignedPermissionIds = projectRole.ProjectRolePermissions
			.Where(prp => prp.DeletedOn == null && prp.ProjectPermission != null && prp.ProjectPermission.DeletedOn == null)
			.Select(prp => prp.ProjectPermissionId)
			.ToList();

		var availablePermissions = await dbContext.ProjectPermissions
			.Where(p => p.DeletedOn == null && !assignedPermissionIds.Contains(p.Id))
			.ToListAsync(cancellationToken);

		return availablePermissions.Select(p => new ProjectPermissionDto
		{
			Id = p.Id,
			Code = p.Code,
			Name = p.Name
		}).ToList();
	}
}
