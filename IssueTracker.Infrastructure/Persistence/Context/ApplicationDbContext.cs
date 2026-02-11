
using Microsoft.EntityFrameworkCore;
using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Domain.Entities;

namespace IssueTracker.Infrastructure.Persistence.Context;

public class ApplicationDbContext(DbContextOptions options, ICurrentUser currentUser) : BaseDbContext(options, currentUser), IApplicationDbContext
{

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

		// Apply all configurations from assembly
		modelBuilder.Entity<User>(entity =>
		{
			entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
			entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
			entity.Property(e => e.PasswordHash).IsRequired();
			entity.Property(e => e.Salt).IsRequired();
			entity.HasIndex(e => e.Username).IsUnique();
			entity.HasIndex(e => e.Email).IsUnique();
		});

		// Set default schema if needed
		// modelBuilder.HasDefaultSchema("your_schema");
	}
}
