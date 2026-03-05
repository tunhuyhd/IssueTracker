# Authorization Architecture - Quick Reference

## Hệ Thống 2 Tầng

```
┌─────────────────────────────────────────────────────────────────┐
│                    SYSTEM-LEVEL (Global)                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   User ──► Role (USER/ADMIN) ──► System Permissions            │
│              │                         │                         │
│              │                         ├─► USER_MANAGE          │
│              │                         ├─► VIEW_ALL_PROJECTS    │
│              │                         └─► SYSTEM_SETTINGS      │
│                                                                  │
│   Attributes: [MustHaveRole], [MustHavePermission]             │
│   Service: ICurrentUser                                         │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                  PROJECT-LEVEL (Context-based)                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   User ──► UserProject ──► ProjectRole ──► ProjectPermissions  │
│              │   │                │                 │           │
│              │   │                │                 ├─► PROJECT_EDIT │
│              │   │                │                 ├─► ISSUE_MANAGE │
│              │   │                │                 └─► VIEW_PROJECT │
│              │   │                │                               │
│              │   └─► Project      └─► ProjectRoleCode:          │
│              │                        - PROJECT_MANAGER          │
│              │                        - PROJECT_ADMIN            │
│              │                        - DEVELOPER                │
│              │                        - TESTER                   │
│              │                        - VIEWER                   │
│              │                                                   │
│   Attributes: [MustBeProjectMember]                            │
│              [MustHaveProjectPermission]                        │
│   Service: IProjectAuthorizationService                         │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Request Flow

### System-Level Authorization

```
┌─────────────┐
│   Request   │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│  JWT Token      │  ◄─── Contains: user_id, role_code, permissions[]
└──────┬──────────┘
       │
       ▼
┌─────────────────────────┐
│ Authentication Filter   │  ◄─── Validates token
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ ICurrentUser populated  │  ◄─── Extract claims
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ System Authorization    │  ◄─── [MustHaveRole] / [MustHavePermission]
│ Filter                  │
└──────┬──────────────────┘
       │
       ├─── ✅ Authorized ──► Execute Controller
       │
       └─── ❌ Not Authorized ──► 401/403
```

### Project-Level Authorization

```
┌─────────────┐
│   Request   │  ◄─── Must include projectId in route
└──────┬──────┘
       │
       ▼
┌─────────────────────────┐
│ System Auth (if any)    │  ◄─── Check system-level first
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ Extract projectId       │  ◄─── From route parameter
│ from route              │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────────────┐
│ IProjectAuthorizationService    │
│ .IsProjectMemberAsync()         │  ◄─── Check membership
└──────┬──────────────────────────┘
       │
       ├─── ❌ Not Member ──► 403 Forbidden
       │
       ▼
┌─────────────────────────────────┐
│ .HasProjectPermissionAsync()    │  ◄─── Check project permissions
└──────┬──────────────────────────┘
       │
       ├─── ✅ Has Permission ──► Execute Controller
       │
       └─── ❌ No Permission ──► 403 Forbidden
```

## System Admin Bypass

```
┌──────────────┐
│ System Admin │  ◄─── Role = ADMIN
└──────┬───────┘
       │
       ├─► System-Level: Has ALL system permissions
       │
       └─► Project-Level: 
           ├─► Automatic member of ALL projects
           └─► Has ALL project permissions in every project
```

## Database Schema

```sql
-- System Level
users
├── id (PK)
├── role_id (FK) ──► roles
│                     ├── id (PK)
│                     ├── code (USER/ADMIN)
│                     └── permissions (M:M)
│                          ├── id (PK)
│                          ├── code
│                          └── name

-- Project Level
user_projects
├── user_id (FK) ──► users
├── project_id (FK) ──► projects
│                        ├── id (PK)
│                        ├── name
│                        └── owner_id
└── project_role_id (FK) ──► project_roles
                              ├── id (PK)
                              ├── code
                              ├── description
                              └── project_permissions (M:M)
                                   ├── id (PK)
                                   ├── code
                                   └── name
```

## Use Cases Matrix

| Scenario | System Attribute | Project Attribute | Check In Logic |
|----------|------------------|-------------------|----------------|
| Login endpoint | ❌ None | ❌ None | ❌ None |
| Get own profile | `[MustBeAuthenticated]` | ❌ None | ❌ None |
| Admin users list | `[MustHavePermission]` | ❌ None | ❌ None |
| View project info | ❌ None | `[MustBeProjectMember]` | ❌ None |
| Edit project | ❌ None | `[MustHaveProjectPermission]` | ❌ None |
| Admin force delete | `[MustHaveRole(Admin)]` | ❌ None | Check ownership |
| Complex operation | `[MustHavePermission]` | `[MustHaveProjectPermission]` | ✅ Both |

## Quick Decision Tree

```
Do you need to protect an endpoint?
│
├─► NO  ──► No attribute needed (public endpoint)
│
└─► YES
    │
    ├─► Only authentication needed?
    │   └─► Use [MustBeAuthenticated]
    │
    ├─► System-wide action (users, settings, etc.)?
    │   │
    │   ├─► Specific role needed?
    │   │   └─► Use [MustHaveRole(RoleCode.XXX)]
    │   │
    │   └─► Specific permission needed? (RECOMMENDED)
    │       └─► Use [MustHavePermission(PermissionCode.XXX)]
    │
    └─► Project-specific action?
        │
        ├─► Just need to be project member?
        │   └─► Use [MustBeProjectMember]
        │
        └─► Need specific project permission? (RECOMMENDED)
            └─► Use [MustHaveProjectPermission(ProjectPermissionCode.XXX)]
```

## Code Comparison

### System-Level
```csharp
// Controller
[HttpPost("users")]
[MustHavePermission(PermissionCode.UserManage)]
public IActionResult CreateUser() { }

// Business Logic
_currentUser.EnsureHasPermission(PermissionCode.UserManage);
```

### Project-Level
```csharp
// Controller
[HttpPut("{projectId}")]
[MustHaveProjectPermission(ProjectPermissionCode.ProjectEdit)]
public IActionResult UpdateProject(Guid projectId) { }

// Business Logic
await _projectAuth.EnsureHasProjectPermissionAsync(
    projectId, 
    ProjectPermissionCode.ProjectEdit, 
    ct);
```

### Combined
```csharp
// Controller
[HttpPost("{projectId}/sync")]
[MustHavePermission(PermissionCode.SystemSettings)]
[MustHaveProjectPermission(ProjectPermissionCode.ProjectEdit)]
public IActionResult SyncProject(Guid projectId) { }

// Business Logic
_currentUser.EnsureHasPermission(PermissionCode.SystemSettings);
await _projectAuth.EnsureHasProjectPermissionAsync(
    projectId, 
    ProjectPermissionCode.ProjectEdit, 
    ct);
```

---

**See full documentation:**
- [System-Level Authorization Guide](./AUTHORIZATION_GUIDE.md)
- [Project-Level Authorization Guide](./PROJECT_AUTHORIZATION_GUIDE.md)
