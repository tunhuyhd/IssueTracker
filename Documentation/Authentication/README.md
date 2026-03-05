# Authentication & Authorization Documentation

Tài liệu về hệ thống xác thực và phân quyền của IssueTracker.

## 📚 Tài Liệu Có Sẵn

### 1. Authorization Guide (System-Level)
**File:** [AUTHORIZATION_GUIDE.md](./AUTHORIZATION_GUIDE.md)  
**Mục đích:** Hướng dẫn chi tiết về hệ thống authorization cấp hệ thống
**Nội dung:**
- Giới thiệu 3 loại attributes: `[MustBeAuthenticated]`, `[MustHaveRole]`, `[MustHavePermission]`
- Hướng dẫn sử dụng từng attribute
- Kết hợp nhiều attributes
- Best practices và anti-patterns
- Ví dụ thực tế
- Testing và troubleshooting

**Đối tượng:** Tất cả developers  
**Khi nào đọc:** Khi cần implement authorization cho endpoints mới

---

### 2. Project Authorization Guide (Project-Level)
**File:** [PROJECT_AUTHORIZATION_GUIDE.md](./PROJECT_AUTHORIZATION_GUIDE.md)  
**Mục đích:** Hướng dẫn chi tiết về hệ thống authorization cấp project  
**Nội dung:**
- Kiến trúc 2 tầng authorization (System + Project)
- Attributes: `[MustBeProjectMember]`, `[MustHaveProjectPermission]`
- IProjectAuthorizationService interface
- Extension methods cho business logic
- Kết hợp system và project permissions
- System admin privileges

**Đối tượng:** Developers làm việc với project features  
**Khi nào đọc:** Khi cần implement project-specific authorization

---

### 3. Authorization Migration Guide
**File:** [AUTHORIZATION_MIGRATION_GUIDE.md](./AUTHORIZATION_MIGRATION_GUIDE.md)  
**Mục đích:** Hướng dẫn migrate code cũ sang hệ thống authorization mới  
**Nội dung:**
- Checklist đầy đủ cho migration
- Các pattern migration phổ biến (Before/After)
- Hướng dẫn từng bước
- Testing migration
- Common issues và solutions

**Đối tượng:** Developers đang migrate dự án  
**Khi nào đọc:** Khi cần refactor code authorization cũ

---

### 4. Authorization Quick Reference
**File:** [README_AUTHORIZATION.md](./README_AUTHORIZATION.md)  
**Mục đích:** Tài liệu tham khảo nhanh  
**Nội dung:**
- Quick start examples
- Available constants (RoleCode, PermissionCode, etc.)
- Common use cases
- Error responses
- Business logic usage
- Troubleshooting quick tips

**Đối tượng:** Tất cả developers  
**Khi nào đọc:** Khi cần tra cứu nhanh syntax hoặc constants

---

## 🎯 Learning Path

### Cho Developer Mới

1. **Bắt đầu với Quick Reference**
   - Đọc [README_AUTHORIZATION.md](./README_AUTHORIZATION.md)
   - Xem phần "Use Cases" để hiểu các scenarios phổ biến

2. **Đọc Authorization Guide**
   - Đọc [AUTHORIZATION_GUIDE.md](./AUTHORIZATION_GUIDE.md) sections 1-3
   - Tập trung vào `[MustHavePermission]` (recommended approach)

3. **Thực hành với Examples**
   - Xem `AuthorizationExampleController.cs`
   - Copy-paste và modify cho use case của bạn

4. **Đọc Best Practices**
   - Đọc [AUTHORIZATION_GUIDE.md](./AUTHORIZATION_GUIDE.md) section 7
   - Áp dụng vào code của bạn

### Cho Developer Đang Migrate

1. **Đánh giá current state**
   - Review code hiện tại
   - Identify manual permission checks

2. **Đọc Migration Guide**
   - Đọc toàn bộ [AUTHORIZATION_MIGRATION_GUIDE.md](./AUTHORIZATION_MIGRATION_GUIDE.md)
   - Note các patterns áp dụng cho project của bạn

3. **Thực hiện Migration**
   - Follow checklist trong migration guide
   - Migrate từng controller một

4. **Testing**
   - Test từng endpoint sau khi migrate
   - Verify 401/403 responses

---

## 🚀 Quick Examples

### Example 1: Protect an Admin Endpoint

```csharp
using IssueTracker.Domain.Entities.Enum;
using IssueTracker.WebApi.Attributes;

[HttpPost("admin/users")]
[MustHavePermission(PermissionCode.UserManage)]
public async Task<IActionResult> CreateUser(CreateUserCommand command)
{
    var result = await Mediator.Send(command);
    return Ok(result);
}
```

**Tài liệu chi tiết:** [AUTHORIZATION_GUIDE.md - Section 3](./AUTHORIZATION_GUIDE.md#3-musthavepermission)

---

### Example 2: Protect Entire Controller

```csharp
[MustHaveRole(RoleCode.Admin)]
public class AdminController : BaseApiController
{
    // All methods require Admin role
    
    [HttpGet("settings")]
    [MustHavePermission(PermissionCode.SystemSettings)]
    public async Task<IActionResult> GetSettings()
    {
        // Need Admin role + SystemSettings permission
    }
}
```

**Tài liệu chi tiết:** [AUTHORIZATION_GUIDE.md - Section 5](./AUTHORIZATION_GUIDE.md#5-áp-dụng-cho-controller)

---

### Example 3: Check Permission in Business Logic

```csharp
using IssueTracker.Application.Common.Extensions;
using IssueTracker.Domain.Entities.Enum;

public class MyCommandHandler
{
    private readonly ICurrentUser _currentUser;

    public async Task Handle(MyCommand request, CancellationToken ct)
    {
        // Check single permission
        _currentUser.EnsureHasPermission(PermissionCode.UserManage);
        
        // Check multiple permissions (OR)
        _currentUser.EnsureHasAnyPermission(
            PermissionCode.UserManage,
            PermissionCode.SystemSettings);
        
        // Check multiple permissions (AND)
        _currentUser.EnsureHasAllPermissions(
            PermissionCode.UserManage,
            PermissionCode.SystemSettings);
    }
}
```

**Tài liệu chi tiết:** [README_AUTHORIZATION.md - Business Logic Usage](./README_AUTHORIZATION.md#business-logic-usage)

---

## 🔑 Key Concepts

### 1. Authentication vs Authorization

**Authentication (Xác thực):**
- Xác định user là ai
- Kiểm tra login credentials
- Tạo JWT token với claims

**Authorization (Phân quyền):**
- Xác định user được làm gì
- Kiểm tra roles và permissions
- Validate trước khi execute action

### 2. Three Levels of Protection

| Level | Attribute | Use When |
|-------|-----------|----------|
| Authentication | `[MustBeAuthenticated]` | Chỉ cần đăng nhập |
| Role-based | `[MustHaveRole]` | Kiểm tra role cố định |
| Permission-based | `[MustHavePermission]` | **Recommended** - Linh hoạt nhất |

### 3. Available Constants

**System Roles:**
- `RoleCode.User` - Regular user
- `RoleCode.Admin` - System administrator

**System Permissions:**
- `PermissionCode.UserManage` - Quản lý users
- `PermissionCode.ViewAllProjects` - Xem tất cả projects
- `PermissionCode.SystemSettings` - Cấu hình hệ thống

**Project Roles:**
- `ProjectRoleCode.ProjectManager`
- `ProjectRoleCode.ProjectAdmin`
- `ProjectRoleCode.Developer`
- `ProjectRoleCode.Tester`
- `ProjectRoleCode.Viewer`

**Project Permissions:**
- `ProjectPermissionCode.ProjectEdit`
- `ProjectPermissionCode.IssueManage`
- `ProjectPermissionCode.ViewProject`

---

## 🐛 Common Issues

### Issue: Attribute không hoạt động

**Symptoms:** Endpoint không check permission dù đã có attribute

**Solutions:**
1. Kiểm tra JWT authentication đã configure trong `Program.cs`
2. Verify `ICurrentUser` đã register trong DI
3. Check middleware order: `UseAuthentication()` phải trước `UseAuthorization()`

**Tài liệu:** [AUTHORIZATION_GUIDE.md - Troubleshooting](./AUTHORIZATION_GUIDE.md#10-troubleshooting)

---

### Issue: 401 Unauthorized với valid token

**Symptoms:** Có token hợp lệ nhưng vẫn nhận 401

**Solutions:**
1. Check token format: `Authorization: Bearer <token>`
2. Verify token chưa expired
3. Kiểm tra JWT configuration khớp với token issuer

**Tài liệu:** [AUTHORIZATION_MIGRATION_GUIDE.md - Common Issues](./AUTHORIZATION_MIGRATION_GUIDE.md#common-issues--solutions)

---

### Issue: 403 Forbidden dù có permission

**Symptoms:** User có permission trong DB nhưng vẫn bị 403

**Solutions:**
1. Re-login để refresh token với permissions mới
2. Check permission code match exactly (case-sensitive)
3. Verify role của user có chứa permission trong DB

**Tài liệu:** [AUTHORIZATION_GUIDE.md - Section 10](./AUTHORIZATION_GUIDE.md#lỗi-user-does-not-have-required-permissions)

---

## 📊 Authorization Flow

```
1. User Login
   ↓
2. JWT Token Generated (with roles & permissions claims)
   ↓
3. Token sent in request header
   ↓
4. Authentication Middleware validates token
   ↓
5. ICurrentUser populated with claims
   ↓
6. Authorization Filter checks attributes
   ↓
7. If authorized → Execute controller action
   If not → Return 401/403
```

---

## 🎨 Best Practices

### ✅ DO

1. **Use Permission over Role**
   ```csharp
   [MustHavePermission(PermissionCode.UserManage)]  // ✅ Flexible
   ```

2. **Use Constants**
   ```csharp
   [MustHavePermission(PermissionCode.UserManage)]  // ✅ Type-safe
   ```

3. **Apply at Method Level**
   ```csharp
   [HttpPost("users")]
   [MustHavePermission(PermissionCode.UserManage)]  // ✅ Clear
   ```

### ❌ DON'T

1. **Hardcode Strings**
   ```csharp
   [MustHavePermission("USER_MANAGE")]  // ❌ Error-prone
   ```

2. **Mix Attribute and Manual Checks**
   ```csharp
   [MustHavePermission(PermissionCode.UserManage)]
   public async Task<IActionResult> Action()
   {
       if (!_currentUser.IsAdmin())  // ❌ Redundant
           return Forbid();
   }
   ```

**Tài liệu đầy đủ:** [AUTHORIZATION_GUIDE.md - Section 7](./AUTHORIZATION_GUIDE.md#7-best-practices)

---

## 📖 Related Documentation

- [Main Documentation](../README.md) - Tổng quan toàn bộ hệ thống
- [Authorization Code Examples](../../IssueTracker.WebApi/Controllers/Examples/AuthorizationExampleController.cs)
- [Business Logic Examples](../../IssueTracker.Application/Examples/ExamplePermissionUsage.cs)

---

**Last Updated:** 2026-03-05  
**Version:** 1.0.0
