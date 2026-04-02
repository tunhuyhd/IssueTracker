using Microsoft.EntityFrameworkCore;
using IssueTracker.Domain.Entities;

namespace IssueTracker.Application.Common;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectRole> ProjectRoles { get; }
    DbSet<ProjectPermission> ProjectPermissions { get; }
    DbSet<UserProject> UserProjects { get; }

    DbSet<ProjectRolePermission> ProjectRolePermissions { get; }

    DbSet<InvitationJoiningProject> Invitations { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}