# Authorization System - Quick Reference

## 🚀 Sử Dụng Nhanh

### 1. Chỉ cần đăng nhập
```csharp
[MustBeAuthenticated]
public IActionResult GetProfile() { }
```

### 2. Cần role cụ thể
```csharp
using IssueTracker.Domain.Entities.Enum;

[MustHaveRole(RoleCode.Admin)]
public IActionResult AdminOnly() { }
```

### 3. Cần permission (KHUYẾN NGHỊ)
```csharp
[MustHavePermission(PermissionCode.UserManage)]
public IActionResult CreateUser() { }
```

### 4. Cần 1 trong nhiều permissions
```csharp
[MustHavePermission(PermissionCode.UserManage, PermissionCode.SystemSettings)]
public IActionResult FlexibleAccess() { }
```

### 5. Cần tất cả permissions
```csharp
[MustHavePermission(
    PermissionCode.UserManage, 
    PermissionCode.SystemSettings,
    RequireAll = true)]
public IActionResult StrictAccess() { }
```

## 📦 Files Created

### Domain Layer
```
IssueTracker.Domain/Entities/Enum/
├── RoleCode.cs              # System role constants
├── ProjectRoleCode.cs       # Project role constants
├── PermissionCode.cs        # System permission constants
└── ProjectPermissionCode.cs # Project permission constants
```

### Application Layer
```
IssueTracker.Application/Common/Extensions/
└── PermissionExtensions.cs  # Helper methods for business logic
```

### WebApi Layer
```
IssueTracker.WebApi/
├── Attributes/
│   ├── MustBeAuthenticatedAttribute.cs
│   ├── MustHavePermissionAttribute.cs
│   └── MustHaveRoleAttribute.cs
└── Filters/
    ├── AuthenticationFilter.cs
    ├── PermissionAuthorizationFilter.cs
    └── RoleAuthorizationFilter.cs
```

### Documentation
```
├── AUTHORIZATION_GUIDE.md          # Hướng dẫn chi tiết
├── AUTHORIZATION_MIGRATION_GUIDE.md # Hướng dẫn migrate
└── README_AUTHORIZATION.md          # Tài liệu này
```

### Examples
```
IssueTracker.WebApi/Controllers/Examples/
└── AuthorizationExampleController.cs

IssueTracker.Application/Examples/
└── ExamplePermissionUsage.cs
```

## 🎯 Use Cases

### Public Endpoint (No auth)
```csharp
[HttpPost("login")]
public IActionResult Login() { }  // No attribute needed
```

### Protected Endpoint (Auth only)
```csharp
[HttpGet("profile")]
[MustBeAuthenticated]
public IActionResult GetProfile() { }
```

### Admin Only
```csharp
[MustHaveRole(RoleCode.Admin)]
public class AdminController : BaseApiController { }
```

### Permission-Based (Best Practice)
```csharp
[HttpPost("users")]
[MustHavePermission(PermissionCode.UserManage)]
public IActionResult CreateUser() { }
```

### Combined Role + Permission
```csharp
[MustHaveRole(RoleCode.Admin)]
[MustHavePermission(PermissionCode.SystemSettings)]
public IActionResult CriticalOperation() { }
```

## 🔧 Business Logic Usage

```csharp
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Extensions;
using IssueTracker.Domain.Entities.Enum;

public class MyCommandHandler
{
    private readonly ICurrentUser _currentUser;

    public async Task Handle()
    {
        // Check permission
        _currentUser.EnsureHasPermission(PermissionCode.UserManage);

        // Check role
        if (_currentUser.IsAdmin())
        {
            // Admin logic
        }

        // Check any permission
        _currentUser.EnsureHasAnyPermission(
            PermissionCode.UserManage,
            PermissionCode.SystemSettings);

        // Check all permissions
        _currentUser.EnsureHasAllPermissions(
            PermissionCode.UserManage,
            PermissionCode.SystemSettings);
    }
}
```

## 📊 Available Constants

### System Roles (RoleCode)
- `RoleCode.User` - "USER"
- `RoleCode.Admin` - "ADMIN"

### System Permissions (PermissionCode)
- `PermissionCode.UserManage` - "USER_MANAGE"
- `PermissionCode.ViewAllProjects` - "VIEW_ALL_PROJECTS"
- `PermissionCode.SystemSettings` - "SYSTEM_SETTINGS"

### Project Roles (ProjectRoleCode)
- `ProjectRoleCode.ProjectManager` - "PROJECT_MANAGER"
- `ProjectRoleCode.ProjectAdmin` - "PROJECT_ADMIN"
- `ProjectRoleCode.Developer` - "DEVELOPER"
- `ProjectRoleCode.Tester` - "TESTER"
- `ProjectRoleCode.Viewer` - "VIEWER"

### Project Permissions (ProjectPermissionCode)
- `ProjectPermissionCode.ProjectEdit` - "PROJECT_EDIT"
- `ProjectPermissionCode.IssueManage` - "ISSUE_MANAGE"
- `ProjectPermissionCode.ViewProject` - "VIEW_PROJECT"

## 🚨 Error Responses

### 401 Unauthorized (Not logged in)
```json
{
  "error": "Unauthorized",
  "message": "User is not authenticated"
}
```

### 403 Forbidden (Logged in but no permission)
```json
{
  "error": "Forbidden",
  "message": "User does not have required permission(s)...",
  "requiredPermissions": ["USER_MANAGE"],
  "requireAll": false
}
```

## ✅ Best Practices

1. **Prefer Permissions over Roles**
   ```csharp
   // ✅ GOOD
   [MustHavePermission(PermissionCode.UserManage)]
   
   // ❌ AVOID
   [MustHaveRole(RoleCode.Admin)]
   ```

2. **Use Constants, Not Strings**
   ```csharp
   // ✅ GOOD
   [MustHavePermission(PermissionCode.UserManage)]
   
   // ❌ AVOID
   [MustHavePermission("USER_MANAGE")]
   ```

3. **Apply at Method Level When Possible**
   ```csharp
   // ✅ GOOD - Clear permissions for each action
   public class UserController : BaseApiController
   {
       [MustHavePermission(PermissionCode.UserManage)]
       public IActionResult CreateUser() { }
       
       [MustBeAuthenticated]
       public IActionResult GetProfile() { }
   }
   ```

4. **Document Why RequireAll = true**
   ```csharp
   // ✅ GOOD - Documented reason
   /// <summary>
   /// Deleting users requires both USER_MANAGE and SYSTEM_SETTINGS
   /// for security reasons
   /// </summary>
   [MustHavePermission(
       PermissionCode.UserManage,
       PermissionCode.SystemSettings,
       RequireAll = true)]
   public IActionResult DeleteUser() { }
   ```

## 🧪 Testing

### With Swagger
1. Click "Authorize" button
2. Enter token: `Bearer <your-token>`
3. Test endpoints

### With Postman/curl
```bash
curl -X GET "https://localhost:5001/api/admin/users" \
  -H "Authorization: Bearer <your-token>"
```

## 📚 More Information

- **Full Guide:** See `AUTHORIZATION_GUIDE.md`
- **Migration:** See `AUTHORIZATION_MIGRATION_GUIDE.md`
- **Examples:** See `AuthorizationExampleController.cs`
- **Business Logic:** See `ExamplePermissionUsage.cs`

## 🆘 Need Help?

### Common Issues

1. **401 with valid token?**
   - Check token format: `Authorization: Bearer <token>`
   - Verify token not expired

2. **403 despite having permission?**
   - Re-login to refresh token
   - Check permission code matches exactly

3. **Attribute not working?**
   - Ensure JWT authentication configured
   - Check ICurrentUser registered in DI

### Quick Debug

```csharp
// In any handler or controller
var userId = _currentUser.GetUserId();
var permissions = _currentUser.GetPermissions();
var roleCode = _currentUser.GetRoleCode();

Console.WriteLine($"User: {userId}");
Console.WriteLine($"Role: {roleCode}");
Console.WriteLine($"Permissions: {string.Join(", ", permissions)}");
```
