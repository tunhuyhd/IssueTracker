# IssueTracker - Documentation

Tài liệu hệ thống IssueTracker Backend API

## 📁 Cấu Trúc Thư Mục

```
Documentation/
├── Authentication/          # Tài liệu về xác thực và phân quyền
│   ├── AUTHORIZATION_GUIDE.md              # Hướng dẫn sử dụng hệ thống authorization
│   ├── AUTHORIZATION_MIGRATION_GUIDE.md    # Hướng dẫn migrate sang hệ thống mới
│   └── README_AUTHORIZATION.md             # Quick reference cho authorization
│
├── Services/               # Tài liệu về các services
│   └── (Chưa có tài liệu)
│
└── Infrastructure/         # Tài liệu về infrastructure
    └── (Chưa có tài liệu)
```

## 📚 Danh Mục Tài Liệu

### 🔐 Authentication & Authorization

#### [Authorization Guide](./Authentication/AUTHORIZATION_GUIDE.md)
Hướng dẫn chi tiết về hệ thống phân quyền:
- Sử dụng `[MustBeAuthenticated]` - Yêu cầu đăng nhập
- Sử dụng `[MustHaveRole]` - Kiểm tra role
- Sử dụng `[MustHavePermission]` - Kiểm tra permission (Khuyến nghị)
- Best practices và ví dụ thực tế

**Đối tượng:** Developers
**Độ khó:** Trung bình

#### [Authorization Migration Guide](./Authentication/AUTHORIZATION_MIGRATION_GUIDE.md)
Hướng dẫn migrate code cũ sang hệ thống authorization mới:
- Checklist migration
- Các pattern phổ biến
- Before/After examples
- Testing và troubleshooting

**Đối tượng:** Developers (khi migrate dự án)
**Độ khó:** Nâng cao

#### [Authorization Quick Reference](./Authentication/README_AUTHORIZATION.md)
Tài liệu tham khảo nhanh cho authorization:
- Các use cases phổ biến
- Code snippets
- Available constants
- Error responses

**Đối tượng:** Developers (tra cứu nhanh)
**Độ khó:** Cơ bản

---

## 🚀 Quick Start

### Cho Developers Mới

1. **Đọc tài liệu cơ bản:**
   - [Authorization Quick Reference](./Authentication/README_AUTHORIZATION.md)

2. **Xem ví dụ code:**
   - `IssueTracker.WebApi/Controllers/Examples/AuthorizationExampleController.cs`
   - `IssueTracker.Application/Examples/ExamplePermissionUsage.cs`

3. **Áp dụng vào project:**
   - Sử dụng `[MustHavePermission]` cho các endpoints cần bảo vệ
   - Sử dụng constants từ `PermissionCode`, `RoleCode`

### Cho Developers Đang Migrate

1. **Đọc migration guide:**
   - [Authorization Migration Guide](./Authentication/AUTHORIZATION_MIGRATION_GUIDE.md)

2. **Thực hiện theo checklist:**
   - Pre-migration steps
   - Migration patterns
   - Post-migration testing

3. **Remove old code:**
   - Xóa manual permission checks
   - Cleanup hardcoded strings

---

## 🎯 Common Tasks

### Task: Bảo vệ một API endpoint

**Scenario:** Tạo endpoint để quản lý users, chỉ Admin có permission `USER_MANAGE` mới được truy cập.

**Solution:**
```csharp
using IssueTracker.Domain.Entities.Enum;
using IssueTracker.WebApi.Attributes;

[HttpPost("users")]
[MustHavePermission(PermissionCode.UserManage)]
public async Task<IActionResult> CreateUser(CreateUserCommand command)
{
    var result = await Mediator.Send(command);
    return Ok(result);
}
```

**Tài liệu:** [Authorization Guide - Section 3](./Authentication/AUTHORIZATION_GUIDE.md#3-musthavepermission)

---

### Task: Kiểm tra permission trong business logic

**Scenario:** Trong command handler cần kiểm tra user có permission trước khi thực thi logic.

**Solution:**
```csharp
using IssueTracker.Application.Common.Extensions;
using IssueTracker.Domain.Entities.Enum;

public class MyCommandHandler
{
    private readonly ICurrentUser _currentUser;

    public async Task Handle()
    {
        _currentUser.EnsureHasPermission(PermissionCode.UserManage);
        // Business logic...
    }
}
```

**Tài liệu:** [Authorization Quick Reference - Business Logic Usage](./Authentication/README_AUTHORIZATION.md#business-logic-usage)

---

### Task: Tạo endpoint với multiple permissions (OR logic)

**Scenario:** User cần có ít nhất 1 trong 2 permissions để truy cập.

**Solution:**
```csharp
[HttpGet("projects")]
[MustHavePermission(
    PermissionCode.ViewAllProjects, 
    ProjectPermissionCode.ViewProject)]
public async Task<IActionResult> GetProjects() { }
```

**Tài liệu:** [Authorization Guide - Section 3](./Authentication/AUTHORIZATION_GUIDE.md#sử-dụng-với-nhiều-permissions---or-logic-mặc-định)

---

### Task: Tạo endpoint yêu cầu ALL permissions

**Scenario:** User phải có tất cả permissions mới được thực hiện action nguy hiểm.

**Solution:**
```csharp
[HttpDelete("users/all")]
[MustHavePermission(
    PermissionCode.UserManage,
    PermissionCode.SystemSettings,
    RequireAll = true)]
public async Task<IActionResult> DeleteAllUsers() { }
```

**Tài liệu:** [Authorization Guide - Section 3](./Authentication/AUTHORIZATION_GUIDE.md#sử-dụng-với-nhiều-permissions---and-logic-yêu-cầu-tất-cả)

---

## 📊 Architecture Overview

### Authorization Flow

```
User Request → JWT Token → Authentication Filter → Permission Filter → Controller
                    ↓              ↓                      ↓
                Claims → ICurrentUser.IsAuthenticated() → ICurrentUser.HasPermission()
```

### Key Components

| Component | Location | Responsibility |
|-----------|----------|---------------|
| `MustHavePermissionAttribute` | `WebApi/Attributes/` | Declarative permission check |
| `PermissionAuthorizationFilter` | `WebApi/Filters/` | Execute permission validation |
| `ICurrentUser` | `Application/Common/Authorization/` | Access current user info |
| `PermissionExtensions` | `Application/Common/Extensions/` | Helper methods for business logic |
| `PermissionCode` | `Domain/Entities/Enum/` | Permission constants |
| `RoleCode` | `Domain/Entities/Enum/` | Role constants |

---

## 🔧 Configuration

### Required Setup

1. **JWT Authentication** (Program.cs)
   ```csharp
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options => { /* ... */ });
   ```

2. **ICurrentUser Registration** (Startup.cs)
   ```csharp
   services.AddScoped<ICurrentUser, CurrentUser>();
   ```

3. **Authorization Middleware** (Program.cs)
   ```csharp
   app.UseAuthentication();
   app.UseAuthorization();
   ```

---

## 🐛 Troubleshooting

### Issue: 401 Unauthorized với valid token

**Possible Causes:**
- Token format không đúng
- Token expired
- JWT configuration sai

**Solution:** Xem [Authorization Guide - Troubleshooting](./Authentication/AUTHORIZATION_GUIDE.md#10-troubleshooting)

---

### Issue: 403 Forbidden nhưng user có permission

**Possible Causes:**
- Token chưa được refresh với permissions mới
- Permission code không khớp
- RequireAll = true nhưng thiếu permission

**Solution:** Xem [Authorization Migration Guide - Common Issues](./Authentication/AUTHORIZATION_MIGRATION_GUIDE.md#common-issues--solutions)

---

## 📝 Contributing

Khi thêm tài liệu mới:

1. **Đặt file vào đúng folder:**
   - Authentication → `Documentation/Authentication/`
   - Services → `Documentation/Services/`
   - Infrastructure → `Documentation/Infrastructure/`

2. **Naming convention:**
   - `{FEATURE}_GUIDE.md` - Hướng dẫn chi tiết
   - `{FEATURE}_MIGRATION_GUIDE.md` - Hướng dẫn migrate
   - `README_{FEATURE}.md` - Quick reference

3. **Cập nhật README.md:**
   - Thêm link vào danh mục
   - Mô tả ngắn gọn
   - Đối tượng và độ khó

---

## 📞 Support

Nếu cần hỗ trợ:
- Kiểm tra [Authorization Guide](./Authentication/AUTHORIZATION_GUIDE.md) cho hướng dẫn chi tiết
- Xem [Examples](../IssueTracker.WebApi/Controllers/Examples/) cho code mẫu
- Tham khảo [Quick Reference](./Authentication/README_AUTHORIZATION.md) cho tra cứu nhanh

---

## 📖 Additional Resources

### Code Examples
- `IssueTracker.WebApi/Controllers/Examples/AuthorizationExampleController.cs`
- `IssueTracker.Application/Examples/ExamplePermissionUsage.cs`

### Domain Models
- `IssueTracker.Domain/Entities/Enum/PermissionCode.cs`
- `IssueTracker.Domain/Entities/Enum/RoleCode.cs`
- `IssueTracker.Domain/Entities/Enum/ProjectRoleCode.cs`
- `IssueTracker.Domain/Entities/Enum/ProjectPermissionCode.cs`

### Implementation
- `IssueTracker.WebApi/Attributes/MustHavePermissionAttribute.cs`
- `IssueTracker.WebApi/Filters/PermissionAuthorizationFilter.cs`
- `IssueTracker.Application/Common/Extensions/PermissionExtensions.cs`

---

**Last Updated:** 2026-03-05  
**Version:** 1.0.0  
**Maintainer:** IssueTracker Development Team
