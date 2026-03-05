# Project-Level Authorization Guide

## Tổng Quan

IssueTracker có **2 tầng phân quyền**:

### 1. System-Level (Global)
```
User → Role (USER/ADMIN) → Permissions (system permissions)
```
- Áp dụng cho toàn hệ thống
- Example: Quản lý users, cấu hình hệ thống

### 2. Project-Level (Context-based)
```
User → Project → ProjectRole → ProjectPermissions
```
- Áp dụng trong context của từng project
- Example: Chỉnh sửa project, quản lý issues

## Kiến Trúc

### Database Schema

```
users
  ├── role_id → roles → permissions (system-level)
  └── user_projects
        ├── project_id → projects
        └── project_role_id → project_roles → project_permissions
```

### Flow

1. **User login** → JWT token chứa system permissions
2. **Access project endpoint** → Check project membership và project permissions
3. **System Admin** → Tự động có tất cả project permissions

## Attributes

### 1. MustBeProjectMember

Chỉ yêu cầu user là member của project.

```csharp
[HttpGet("{projectId}/info")]
[MustBeProjectMember]
public IActionResult GetProjectInfo(Guid projectId)
{
    // Only project members can access
    return Ok();
}
```

**Parameters:**
- `projectIdParameterName` (optional): Tên của route parameter chứa projectId (default: "projectId")

**Example with custom parameter:**
```csharp
[HttpGet("project/{id}/info")]
[MustBeProjectMember("id")]
public IActionResult GetInfo(Guid id) { }
```

---

### 2. MustHaveProjectPermission

Yêu cầu user có project permission cụ thể.

#### Single Permission:
```csharp
[HttpPut("{projectId}")]
[MustHaveProjectPermission(ProjectPermissionCode.ProjectEdit)]
public IActionResult UpdateProject(Guid projectId)
{
    return Ok();
}
```

#### Multiple Permissions (OR logic - default):
```csharp
[HttpGet("{projectId}/dashboard")]
[MustHaveProjectPermission(
    ProjectPermissionCode.ProjectEdit,
    ProjectPermissionCode.IssueManage)]
public IActionResult GetDashboard(Guid projectId)
{
    // User needs PROJECT_EDIT OR ISSUE_MANAGE
    return Ok();
}
```

#### Multiple Permissions (AND logic):
```csharp
[HttpDelete("{projectId}")]
[MustHaveProjectPermission(
    ProjectPermissionCode.ProjectEdit,
    ProjectPermissionCode.IssueManage,
    RequireAll = true)]
public IActionResult DeleteProject(Guid projectId)
{
    // User needs BOTH permissions
    return Ok();
}
```

**Parameters:**
- `permissionCode(s)`: Project permission codes
- `RequireAll` (optional): true = AND logic, false = OR logic (default)
- `ProjectIdParameterName` (optional): Route parameter name (default: "projectId")

---

## Business Logic Usage

### Service Interface

`IProjectAuthorizationService` cung cấp các methods:

```csharp
// Check membership
Task<bool> IsProjectMemberAsync(Guid projectId);
Task<bool> IsProjectOwnerAsync(Guid projectId);

// Get user's role and permissions
Task<string?> GetProjectRoleAsync(Guid projectId);
Task<string[]> GetProjectPermissionsAsync(Guid projectId);

// Check permissions
Task<bool> HasProjectPermissionAsync(Guid projectId, string permissionCode);
Task<bool> HasAnyProjectPermissionAsync(Guid projectId, string[] permissionCodes);
Task<bool> HasAllProjectPermissionsAsync(Guid projectId, string[] permissionCodes);

// Ensure (throw exception if not authorized)
Task EnsureHasProjectPermissionAsync(Guid projectId, string permissionCode);
Task EnsureIsProjectMemberAsync(Guid projectId);
```

### Example 1: Simple Permission Check

```csharp
public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, bool>
{
    private readonly IProjectAuthorizationService _projectAuth;

    public async Task<bool> Handle(UpdateProjectCommand request, CancellationToken ct)
    {
        // Check permission
        await _projectAuth.EnsureHasProjectPermissionAsync(
            request.ProjectId,
            ProjectPermissionCode.ProjectEdit,
            ct);

        // Business logic...
        return true;
    }
}
```

### Example 2: Using Extension Methods

```csharp
using IssueTracker.Application.Common.Extensions;

public class CreateIssueCommandHandler : IRequestHandler<CreateIssueCommand, Guid>
{
    private readonly IProjectAuthorizationService _projectAuth;

    public async Task<Guid> Handle(CreateIssueCommand request, CancellationToken ct)
    {
        // Use convenient extension method
        await _projectAuth.EnsureCanManageIssuesAsync(request.ProjectId, ct);

        // Business logic...
        return Guid.NewGuid();
    }
}
```

### Example 3: Conditional Logic

```csharp
public async Task<ProjectDataDto> Handle(GetProjectDataCommand request, CancellationToken ct)
{
    // Ensure user is project member
    await _projectAuth.EnsureIsProjectMemberAsync(request.ProjectId, ct);

    var result = new ProjectDataDto();

    // Show different data based on permissions
    if (await _projectAuth.CanEditProjectAsync(request.ProjectId, ct))
    {
        result.AdminFeatures = true;
    }

    if (await _projectAuth.CanManageIssuesAsync(request.ProjectId, ct))
    {
        result.ManagementFeatures = true;
    }

    return result;
}
```

### Example 4: Check Ownership

```csharp
public async Task<bool> Handle(TransferProjectCommand request, CancellationToken ct)
{
    // Only owner or system admin can transfer
    var isOwner = await _projectAuth.IsProjectOwnerAsync(request.ProjectId, ct);
    var isAdmin = _currentUser.IsAdmin();

    if (!isOwner && !isAdmin)
    {
        throw new UnauthorizedAccessException("Only owner or admin can transfer");
    }

    // Business logic...
    return true;
}
```

---

## Extension Methods

`ProjectAuthorizationExtensions` cung cấp helper methods:

```csharp
// Ensure methods (throw exception)
await _projectAuth.EnsureHasProjectPermissionAsync(projectId, permissionCode, ct);
await _projectAuth.EnsureHasAnyProjectPermissionAsync(projectId, permissionCodes, ct);
await _projectAuth.EnsureHasAllProjectPermissionsAsync(projectId, permissionCodes, ct);

// Convenient permission checks
await _projectAuth.CanEditProjectAsync(projectId, ct);
await _projectAuth.CanManageIssuesAsync(projectId, ct);
await _projectAuth.CanViewProjectAsync(projectId, ct);

// Convenient ensure methods
await _projectAuth.EnsureCanEditProjectAsync(projectId, ct);
await _projectAuth.EnsureCanManageIssuesAsync(projectId, ct);
```

---

## Available Project Permissions

Defined in `ProjectPermissionCode`:

```csharp
ProjectPermissionCode.ProjectEdit    // Chỉnh sửa project
ProjectPermissionCode.IssueManage    // Quản lý issues
ProjectPermissionCode.ViewProject    // Xem project (read-only)
```

## Available Project Roles

Defined in `ProjectRoleCode`:

```csharp
ProjectRoleCode.ProjectManager  // Quản lý dự án
ProjectRoleCode.ProjectAdmin    // Quản trị dự án
ProjectRoleCode.Developer       // Nhà phát triển
ProjectRoleCode.Tester          // Người kiểm thử
ProjectRoleCode.Viewer          // Người xem
```

---

## Combining System and Project Authorization

You can combine both levels:

### Example 1: System Role + Project Membership
```csharp
[HttpPost("{projectId}/advanced")]
[MustHaveRole(RoleCode.Admin)]
[MustBeProjectMember]
public IActionResult AdvancedFeature(Guid projectId)
{
    // Must be Admin AND project member
    return Ok();
}
```

### Example 2: System Permission + Project Permission
```csharp
[HttpPost("{projectId}/sync")]
[MustHavePermission(PermissionCode.SystemSettings)]
[MustHaveProjectPermission(ProjectPermissionCode.ProjectEdit)]
public IActionResult SyncProject(Guid projectId)
{
    // Must have system permission AND project permission
    return Ok();
}
```

### Example 3: In Business Logic
```csharp
public async Task<bool> Handle(ArchiveProjectCommand request, CancellationToken ct)
{
    // System admin can archive any project
    if (_currentUser.HasPermission(PermissionCode.SystemSettings))
    {
        // Bypass project permission check
    }
    else
    {
        // Regular user must have project permission
        await _projectAuth.EnsureHasProjectPermissionAsync(
            request.ProjectId,
            ProjectPermissionCode.ProjectEdit,
            ct);
    }

    // Business logic...
    return true;
}
```

---

## System Admin Privileges

**System Admins (role = ADMIN) automatically have:**
- Access to ALL projects
- ALL project permissions in every project
- Bypass project membership checks

This is implemented in `ProjectAuthorizationService`:
```csharp
// If user is system admin, grant all project permissions
if (_currentUser.GetRoleCode() == RoleCode.Admin)
{
    return allPermissions;
}
```

---

## Response Codes

### 400 Bad Request
Missing or invalid projectId parameter:
```json
{
  "error": "BadRequest",
  "message": "Project ID parameter 'projectId' is missing or invalid"
}
```

### 401 Unauthorized
User not authenticated:
```json
{
  "error": "Unauthorized",
  "message": "User is not authenticated"
}
```

### 403 Forbidden

**Not a project member:**
```json
{
  "error": "Forbidden",
  "message": "User is not a member of this project",
  "projectId": "..."
}
```

**Missing project permission:**
```json
{
  "error": "Forbidden",
  "message": "User does not have required project permission(s). Required at least one of: PROJECT_EDIT",
  "projectId": "...",
  "requiredPermissions": ["PROJECT_EDIT"],
  "requireAll": false
}
```

---

## Best Practices

### ✅ DO

1. **Use Project Permissions for Project-Specific Actions**
   ```csharp
   // ✅ GOOD
   [MustHaveProjectPermission(ProjectPermissionCode.ProjectEdit)]
   ```

2. **Check Membership First in Business Logic**
   ```csharp
   // ✅ GOOD
   await _projectAuth.EnsureIsProjectMemberAsync(projectId, ct);
   ```

3. **Use Extension Methods**
   ```csharp
   // ✅ GOOD - Clean and readable
   await _projectAuth.EnsureCanEditProjectAsync(projectId, ct);
   ```

4. **Allow System Admin to Bypass**
   ```csharp
   // ✅ GOOD - System admin can do anything
   if (!_currentUser.IsAdmin())
   {
       await _projectAuth.EnsureHasProjectPermissionAsync(...);
   }
   ```

### ❌ DON'T

1. **Don't Use System Permissions for Project Actions**
   ```csharp
   // ❌ BAD - Should use project permission
   [MustHavePermission(PermissionCode.UserManage)]
   public IActionResult UpdateProject(Guid projectId) { }
   ```

2. **Don't Forget to Check Membership**
   ```csharp
   // ❌ BAD - What if user is not a member?
   public async Task Handle(Command request, CancellationToken ct)
   {
       // Missing membership check!
       // Do something with project...
   }
   ```

3. **Don't Hardcode Permission Strings**
   ```csharp
   // ❌ BAD
   [MustHaveProjectPermission("PROJECT_EDIT")]
   
   // ✅ GOOD
   [MustHaveProjectPermission(ProjectPermissionCode.ProjectEdit)]
   ```

---

## Testing

### Test Scenarios

1. **Non-member accessing project**
   - Expected: 403 Forbidden (not a member)

2. **Member without permission**
   - Expected: 403 Forbidden (no permission)

3. **Member with permission**
   - Expected: 200 OK

4. **System admin**
   - Expected: 200 OK (bypass all checks)

### Example Test Flow

```bash
# 1. Login as regular user
POST /api/auth/login
→ Get JWT token

# 2. Try to access project (not a member)
GET /api/projects/{projectId}/info
Authorization: Bearer <token>
→ Expected: 403 (not a member)

# 3. Join project (or be added as member)
POST /api/projects/{projectId}/members
→ User added with VIEWER role

# 4. Try to access again
GET /api/projects/{projectId}/info
→ Expected: 200 OK (now a member)

# 5. Try to edit (no permission)
PUT /api/projects/{projectId}
→ Expected: 403 (VIEWER doesn't have PROJECT_EDIT)

# 6. Login as admin
POST /api/auth/login (admin credentials)
→ Get admin JWT token

# 7. Try to edit
PUT /api/projects/{projectId}
Authorization: Bearer <admin_token>
→ Expected: 200 OK (admin bypasses checks)
```

---

## Migration from Old Code

### Before (Manual checks)
```csharp
public async Task<IActionResult> UpdateProject(Guid projectId)
{
    var user = await _context.Users.FindAsync(_currentUser.GetUserId());
    var userProject = await _context.UserProjects
        .Include(up => up.ProjectRole)
        .ThenInclude(pr => pr.ProjectPermissions)
        .FirstOrDefaultAsync(up => up.UserId == user.Id && up.ProjectId == projectId);
    
    if (userProject == null)
        return Forbid();
    
    if (!userProject.ProjectRole.ProjectPermissions.Any(p => p.Code == "PROJECT_EDIT"))
        return Forbid();
    
    // Business logic...
}
```

### After (Using attributes)
```csharp
[MustHaveProjectPermission(ProjectPermissionCode.ProjectEdit)]
public async Task<IActionResult> UpdateProject(Guid projectId)
{
    // Business logic...
}
```

---

## Related Documentation

- [System-Level Authorization Guide](./AUTHORIZATION_GUIDE.md)
- [Authorization Quick Reference](./README_AUTHORIZATION.md)
- [Controller Examples](../../IssueTracker.WebApi/Controllers/Examples/ProjectAuthorizationExampleController.cs)
- [Business Logic Examples](../../IssueTracker.Application/Examples/ProjectAuthorizationExamples.cs)

---

**Last Updated:** 2026-03-05  
**Version:** 1.0.0
