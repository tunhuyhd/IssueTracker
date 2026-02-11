using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Common;
using IssueTracker.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace IssueTracker.Infrastructure.Persistence.Context;

public abstract class BaseDbContext : DbContext
{
    private readonly ICurrentUser CurrentUser;

	protected BaseDbContext(DbContextOptions options, ICurrentUser currentUser) : base(options)
	{
		CurrentUser = currentUser;
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
	}

	public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var userId = CurrentUser.GetUserId();
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = userId;
                    // CreatedOn is set in constructor
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = userId;
                    entry.Entity.LastModifiedOn = now;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.DeletedOn = now;
                entry.Entity.DeletedBy = userId;
            }
        }
    }
}
