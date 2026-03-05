# Hướng Dẫn Sử Dụng Hệ Thống Authorization

## Tổng Quan

Hệ thống authorization cung cấp 3 loại attributes để bảo vệ API endpoints:

1. **`[MustBeAuthenticated]`** - Chỉ yêu cầu user đã đăng nhập
2. **`[MustHaveRole]`** - Yêu cầu user có role cụ thể
3. **`[MustHavePermission]`** - Yêu cầu user có permission cụ thể

## 1. MustBeAuthenticated

Sử dụng khi chỉ cần kiểm tra user đã đăng nhập, không quan tâm role hay permission.

```csharp
[HttpGet("profile")]
[MustBeAuthenticated]
public async Task<IActionResult> GetProfile()
{
    // Chỉ user đã đăng nhập mới truy cập được
    return Ok();
}
```

## 2. MustHaveRole

Sử dụng khi cần kiểm tra user có role cụ thể.

### Sử dụng với 1 role:

```csharp
using IssueTracker.Domain.Entities.Enum;

[HttpGet("admin-dashboard")]
[MustHaveRole(RoleCode.Admin)]
public async Task<IActionResult> GetAdminDashboard()
{
    // Chỉ user có role ADMIN mới truy cập được
    return Ok();
}
```

### Sử dụng với nhiều roles (user cần có ít nhất 1 trong các role):

```csharp
[HttpGet("management-dashboard")]
[MustHaveRole(RoleCode.Admin, RoleCode.User)]
public async Task<IActionResult> GetManagementDashboard()
{
    // User có role ADMIN hoặc USER đều truy cập được
    return Ok();
}
```

## 3. MustHavePermission

Sử dụng khi cần kiểm tra user có permission cụ thể. **ĐÂY LÀ CÁCH ĐƯỢC KHUYẾN NGHỊ** để kiểm soát quyền truy cập.

### Sử dụng với 1 permission:

```csharp
using IssueTracker.Domain.Entities.Enum;

[HttpPost("users")]
[MustHavePermission(PermissionCode.UserManage)]
public async Task<IActionResult> CreateUser(CreateUserCommand command)
{
    // Chỉ user có permission USER_MANAGE mới được tạo user
    var result = await Mediator.Send(command);
    return Ok(result);
}
```

### Sử dụng với nhiều permissions - OR logic (mặc định):

```csharp
[HttpGet("projects")]
[MustHavePermission(PermissionCode.ViewAllProjects, ProjectPermissionCode.ViewProject)]
public async Task<IActionResult> GetProjects()
{
    // User có 1 trong 2 permissions đều truy cập được
    return Ok();
}
```

### Sử dụng với nhiều permissions - AND logic (yêu cầu tất cả):

```csharp
[HttpDelete("users/{id}")]
[MustHavePermission(PermissionCode.UserManage, PermissionCode.SystemSettings, RequireAll = true)]
public async Task<IActionResult> DeleteUser(Guid id)
{
    // User phải có CẢ 2 permissions mới được xóa user
    return Ok();
}
```

## 4. Kết Hợp Nhiều Attributes

Bạn có thể kết hợp nhiều attributes để tạo logic phức tạp hơn:

```csharp
[HttpPost("critical-operation")]
[MustHaveRole(RoleCode.Admin)]  // Phải là Admin
[MustHavePermission(PermissionCode.SystemSettings)]  // VÀ phải có permission SYSTEM_SETTINGS
public async Task<IActionResult> CriticalOperation()
{
    // Cả 2 điều kiện phải thỏa mãn
    return Ok();
}
```

## 5. Áp Dụng Cho Controller

Bạn có thể áp dụng attribute cho cả controller, tất cả methods trong controller sẽ kế thừa:

```csharp
[MustHaveRole(RoleCode.Admin)]  // Áp dụng cho tất cả methods
public class AdminController : BaseApiController
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        // Kế thừa [MustHaveRole(RoleCode.Admin)]
        return Ok();
    }

    [HttpPost("users")]
    [MustHavePermission(PermissionCode.UserManage)]  // Thêm permission check
    public async Task<IActionResult> CreateUser()
    {
        // Phải có role Admin VÀ permission UserManage
        return Ok();
    }
}
```

## 6. Response Codes

### 401 Unauthorized
Khi user chưa đăng nhập:
```json
{
    "error": "Unauthorized",
    "message": "User is not authenticated"
}
```

### 403 Forbidden
Khi user không có quyền (thiếu role hoặc permission):

**Thiếu Role:**
```json
{
    "error": "Forbidden",
    "message": "User does not have required role(s). Required one of: ADMIN",
    "requiredRoles": ["ADMIN"],
    "userRole": "USER"
}
```

**Thiếu Permission:**
```json
{
    "error": "Forbidden",
    "message": "User does not have required permission(s). Required at least one of: USER_MANAGE",
    "requiredPermissions": ["USER_MANAGE"],
    "requireAll": false
}
```

## 7. Best Practices

### ✅ Nên:

1. **Ưu tiên dùng Permission hơn Role**
   ```csharp
   // TỐT - Linh hoạt, dễ mở rộng
   [MustHavePermission(PermissionCode.UserManage)]
   
   // TRÁNH - Cứng nhắc
   [MustHaveRole(RoleCode.Admin)]
   ```

2. **Sử dụng constants từ enum**
   ```csharp
   // TỐT
   [MustHavePermission(PermissionCode.UserManage)]
   
   // TRÁNH - Hardcode
   [MustHavePermission("USER_MANAGE")]
   ```

3. **Đặt attribute ở level thấp nhất có thể**
   ```csharp
   // TỐT - Rõ ràng permission cho từng action
   public class UserController : BaseApiController
   {
       [MustHavePermission(PermissionCode.UserManage)]
       public async Task<IActionResult> CreateUser() { }
       
       [MustBeAuthenticated]
       public async Task<IActionResult> GetProfile() { }
   }
   ```

4. **Sử dụng RequireAll = true khi cần bảo mật cao**
   ```csharp
   [MustHavePermission(
       PermissionCode.UserManage, 
       PermissionCode.SystemSettings, 
       RequireAll = true)]
   public async Task<IActionResult> DeleteAllUsers() { }
   ```

### ❌ Tránh:

1. **Không dùng attribute cho public endpoints**
   ```csharp
   // TỐT - Không có attribute
   [HttpPost("login")]
   public async Task<IActionResult> Login() { }
   
   // TRÁNH
   [MustBeAuthenticated]
   [HttpPost("login")]
   public async Task<IActionResult> Login() { }
   ```

2. **Không mix logic authorization trong code**
   ```csharp
   // TRÁNH
   [MustHavePermission(PermissionCode.ViewAllProjects)]
   public async Task<IActionResult> GetProjects()
   {
       if (_currentUser.GetRoleCode() != RoleCode.Admin)  // ❌ Redundant
           return Forbid();
       // ...
   }
   ```

## 8. Ví Dụ Thực Tế

### Admin Controller
```csharp
using IssueTracker.Domain.Entities.Enum;

[MustHaveRole(RoleCode.Admin)]  // Tất cả actions cần role Admin
public class AdminController : BaseApiController
{
    [HttpGet("users")]
    [MustHavePermission(PermissionCode.UserManage)]
    public async Task<IActionResult> GetUsers() { }

    [HttpPost("users")]
    [MustHavePermission(PermissionCode.UserManage)]
    public async Task<IActionResult> CreateUser() { }

    [HttpGet("settings")]
    [MustHavePermission(PermissionCode.SystemSettings)]
    public async Task<IActionResult> GetSettings() { }
}
```

### Project Controller
```csharp
using IssueTracker.Domain.Entities.Enum;

public class ProjectController : BaseApiController
{
    [HttpGet]
    [MustBeAuthenticated]  // Chỉ cần đăng nhập
    public async Task<IActionResult> GetMyProjects() { }

    [HttpPost]
    [MustHavePermission(ProjectPermissionCode.ProjectEdit)]
    public async Task<IActionResult> CreateProject() { }

    [HttpDelete("{id}")]
    [MustHavePermission(
        ProjectPermissionCode.ProjectEdit,
        PermissionCode.ViewAllProjects,
        RequireAll = true)]
    public async Task<IActionResult> DeleteProject(Guid id) { }
}
```

## 9. Testing

Khi test API với Swagger hoặc Postman:

1. **Không có token** → 401 Unauthorized
2. **Có token nhưng thiếu permission** → 403 Forbidden
3. **Có token và đủ permission** → 200 OK

## 10. Troubleshooting

### Lỗi: "User is not authenticated" nhưng đã gửi token

**Nguyên nhân:** Token không hợp lệ hoặc expired

**Giải pháp:**
- Kiểm tra token có còn hạn không
- Đảm bảo gửi token đúng format: `Authorization: Bearer <token>`

### Lỗi: "User does not have required permission(s)"

**Nguyên nhân:** User chưa được assign permission cần thiết

**Giải pháp:**
- Kiểm tra permissions của user trong database
- Đảm bảo Role của user có chứa Permission cần thiết
- Re-login để refresh token với permissions mới

### Attribute không hoạt động

**Nguyên nhân:** 
- Thiếu `[Authorize]` ở Program.cs
- ICurrentUser không được inject đúng

**Giải pháp:**
- Đảm bảo authentication middleware được configure trong Program.cs
- Kiểm tra ICurrentUser đã được register trong DI container
