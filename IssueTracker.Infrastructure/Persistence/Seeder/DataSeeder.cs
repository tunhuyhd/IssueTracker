using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
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
        await SeedProjectRolesAsync(context);
        await SeedProjectPermissionsAsync(context);
        await SeedAdminAccount(context);


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
                Code = RoleCode.User,
                Description = "Người dùng thông thường",
                CreatedBy = Guid.Empty
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Code = RoleCode.Admin,
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
            .FirstOrDefaultAsync(r => r.Code == RoleCode.Admin);

        if (adminRole == null)
            return;

        // Get permissions to assign to ADMIN
        var permissionCodes = new[] { PermissionCode.UserManage, PermissionCode.ViewAllProjects, PermissionCode.SystemSettings };

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

    public static async Task SeedProjectRolesAsync(ApplicationDbContext context)
    {
        var projectRoles = new List<ProjectRole>
        {
            new ProjectRole
            {
                Id = Guid.NewGuid(),
                Code = "PROJECT_MANAGER",
                Description = "Quản lý dự án",
                CreatedBy = Guid.Empty
            },
			new ProjectRole
			{
				Id = Guid.NewGuid(),
				Code = "PROJECT_ADMIN",
				Description = "Quản trị dự án",
				CreatedBy = Guid.Empty
			},
			new ProjectRole
            {
                Id = Guid.NewGuid(),
                Code = "DEVELOPER",
                Description = "Nhà phát triển",
                CreatedBy = Guid.Empty
            },
            new ProjectRole
            {
                Id = Guid.NewGuid(),
                Code = "TESTER",
                Description = "Người kiểm thử",
                CreatedBy = Guid.Empty
            },
			new ProjectRole
			{
				Id = Guid.NewGuid(),
				Code = "VIEWER",
				Description = "Người xem",
				CreatedBy = Guid.Empty
			}
		};
        foreach (var projectRole in projectRoles)
        {
            var existingProjectRole = await context.ProjectRoles
                .FirstOrDefaultAsync(pr => pr.Code == projectRole.Code);
            if (existingProjectRole == null)
            {
                context.ProjectRoles.Add(projectRole);
            }
        }
        await context.SaveChangesAsync();
	}
	private static async Task SeedProjectPermissionsAsync(ApplicationDbContext context)
    {
		var projectPermissions = new List<ProjectPermission>
        {
            // ================= PROJECT =================
            new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "PROJECT_VIEW",
		        Name = "Xem dự án",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "PROJECT_CREATE",
		        Name = "Tạo dự án",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "PROJECT_EDIT",
		        Name = "Chỉnh sửa dự án",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "PROJECT_DELETE",
		        Name = "Xóa dự án",
		        CreatedBy = Guid.Empty
	        },

            // ================= ISSUE =================
            new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "ISSUE_VIEW",
		        Name = "Xem issue",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "ISSUE_CREATE",
		        Name = "Tạo issue",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "ISSUE_EDIT",
		        Name = "Chỉnh sửa issue",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "ISSUE_DELETE",
		        Name = "Xóa issue",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "ISSUE_ASSIGN",
		        Name = "Gán người xử lý",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "ISSUE_CHANGE_STATUS",
		        Name = "Thay đổi trạng thái issue",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "ISSUE_COMMENT",
		        Name = "Bình luận issue",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "ISSUE_ATTACH_FILE",
		        Name = "Đính kèm file",
		        CreatedBy = Guid.Empty
	        },

            // ================= MEMBER =================
            new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "MEMBER_VIEW",
		        Name = "Xem danh sách thành viên",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "MEMBER_ADD",
		        Name = "Thêm thành viên",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "MEMBER_REMOVE",
		        Name = "Xóa thành viên",
		        CreatedBy = Guid.Empty
	        },
	        new ProjectPermission
	        {
		        Id = Guid.NewGuid(),
		        Code = "MEMBER_UPDATE_ROLE",
		        Name = "Cập nhật vai trò thành viên",
		        CreatedBy = Guid.Empty
	        }
        };

		foreach (var projectPermission in projectPermissions)
        {
            var existingProjectPermission = await context.ProjectPermissions
                .FirstOrDefaultAsync(pp => pp.Code == projectPermission.Code);
            if (existingProjectPermission == null)
            {
                context.ProjectPermissions.Add(projectPermission);
            }
        }
        await context.SaveChangesAsync();
	}

    public static async Task SeedAdminAccount(ApplicationDbContext context)
    {
        var adminEmail = "nguyentunghuy02@gmail.com";
        var existingAdmin = await context.Users
            .FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (existingAdmin == null)
        {
            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Code == "ADMIN");
            if (adminRole == null)
                return;
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = adminEmail,
                FullName = "Admin System",
                RoleId = adminRole.Id,
                IsActive = true,
                CreatedBy = Guid.Empty
            };
            adminUser.SetPasswordHash("Admin@123");
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }
    }
}