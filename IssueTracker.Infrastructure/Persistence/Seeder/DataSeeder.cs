using IssueTracker.Domain.Entities;
using IssueTracker.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Infrastructure.Persistence.Seeder;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Permissions first
        await SeedPermissionsAsync(context);

        // Seed Roles 
        await SeedRolesAsync(context);

        // Assign permissions to roles
        await AssignPermissionsToRolesAsync(context);
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context)
    {
        var permissions = new List<Permission>
        {
            new Permission
            {
                Id = Guid.NewGuid(),
                Code = "USER_MANAGE",
                Name = "Quản lý user hệ thống",
                CreatedBy = Guid.Empty
            },
            new Permission
            {
                Id = Guid.NewGuid(),
                Code = "VIEW_ALL_PROJECTS",
                Name = "Admin xem mọi project",
                CreatedBy = Guid.Empty
            },
            new Permission
            {
                Id = Guid.NewGuid(),
                Code = "SYSTEM_SETTINGS",
                Name = "Cấu hình hệ thống",
                CreatedBy = Guid.Empty
            }
        };

        foreach (var permission in permissions)
        {
            var existingPermission = await context.Permissions
                .FirstOrDefaultAsync(p => p.Code == permission.Code);

            if (existingPermission == null)
            {
                context.Permissions.Add(permission);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.NewGuid(),
                Code = "USER",
                Description = "Người dùng thông thường",
                CreatedBy = Guid.Empty
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Code = "ADMIN",
                Description = "Quản trị viên hệ thống",
                CreatedBy = Guid.Empty
            }
        };

        foreach (var role in roles)
        {
            var existingRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Code == role.Code);

            if (existingRole == null)
            {
                context.Roles.Add(role);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task AssignPermissionsToRolesAsync(ApplicationDbContext context)
    {
        // Get ADMIN role
        var adminRole = await context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Code == "ADMIN");

        if (adminRole == null)
            return;

        // Get permissions to assign to ADMIN
        var permissionCodes = new[] { "USER_MANAGE", "VIEW_ALL_PROJECTS", "SYSTEM_SETTINGS" };

        var permissions = await context.Permissions
            .Where(p => permissionCodes.Contains(p.Code))
            .ToListAsync();

        // Clear existing permissions for admin (if any)
        adminRole.Permissions.Clear();

        // Assign permissions to admin role
        foreach (var permission in permissions)
        {
            adminRole.Permissions.Add(permission);
        }

        await context.SaveChangesAsync();
    }
}