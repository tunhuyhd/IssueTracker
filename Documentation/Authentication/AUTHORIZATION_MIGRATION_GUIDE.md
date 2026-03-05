# Migration Guide: Implementing Authorization System

## Tóm Tắt

Hệ thống authorization mới cung cấp:
- ✅ Type-safe permission checks
- ✅ Declarative authorization với attributes
- ✅ Consistent error responses
- ✅ Flexible permission logic (AND/OR)
- ✅ Support cho cả controller và business logic

## Bước 1: Update Dependencies

Đảm bảo các files sau đã được tạo:

### WebApi Project
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

### Application Project
```
IssueTracker.Application/
└── Common/
    └── Extensions/
        └── PermissionExtensions.cs
```

### Domain Project
```
IssueTracker.Domain/
└── Entities/
    └── Enum/
        ├── RoleCode.cs
        ├── PermissionCode.cs
        ├── ProjectRoleCode.cs
        └── ProjectPermissionCode.cs
```

## Bước 2: Migrate Controllers

### Before (Cách cũ - KHÔNG KHUYẾN NGHỊ)
```csharp
public class AdminController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        // Manual permission check trong code
        var user = await _currentUser.GetCurrentUserAsync();
        if (user.Role.Code != "ADMIN")
        {
            return Forbid();
        }
        
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
```

### After (Cách mới - KHUYẾN NGHỊ)
```csharp
using IssueTracker.Domain.Entities.Enum;
using IssueTracker.WebApi.Attributes;

[MustHaveRole(RoleCode.Admin)]  // Controller-level
public class AdminController : BaseApiController
{
    [HttpPost("users")]
    [MustHavePermission(PermissionCode.UserManage)]  // Method-level
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        // No manual permission check needed!
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
```

## Bước 3: Migrate Business Logic

### Before (Cách cũ)
```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly ICurrentUserService _currentUserService;
    
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var currentUser = await _currentUserService.GetCurrentUserWithRoleAsync(ct);
        
        // Hardcoded permission check
        if (!currentUser.Role.Permissions.Any(p => p.Code == "USER_MANAGE"))
        {
            throw new UnauthorizedAccessException("No permission");
        }
        
        // Business logic...
    }
}
```

### After (Cách mới)
```csharp
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Extensions;
using IssueTracker.Domain.Entities.Enum;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly ICurrentUser _currentUser;
    
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Type-safe permission check
        _currentUser.EnsureHasPermission(PermissionCode.UserManage);
        
        // Business logic...
    }
}
```

## Bước 4: Common Migration Patterns

### Pattern 1: Remove Manual Role Checks

#### Before
```csharp
[HttpGet]
public async Task<IActionResult> GetData()
{
    var user = await _userService.GetCurrentUserAsync();
    if (user.Role.Code != "ADMIN" && user.Role.Code != "MANAGER")
    {
        return Forbid();
    }
    // ...
}
```

#### After
```csharp
[HttpGet]
[MustHaveRole(RoleCode.Admin, RoleCode.Manager)]
public async Task<IActionResult> GetData()
{
    // No manual check needed
    // ...
}
```

### Pattern 2: Replace String-Based Permission Checks

#### Before
```csharp
if (!currentUser.Permissions.Any(p => p.Code == "USER_MANAGE"))
{
    throw new Exception("No permission");
}
```

#### After
```csharp
_currentUser.EnsureHasPermission(PermissionCode.UserManage);
```

### Pattern 3: Complex Permission Logic

#### Before
```csharp
var hasPermission = user.Permissions.Any(p => 
    p.Code == "USER_MANAGE" || p.Code == "SYSTEM_SETTINGS");
    
if (!hasPermission)
{
    throw new UnauthorizedAccessException();
}
```

#### After
```csharp
// In Controller
[MustHavePermission(PermissionCode.UserManage, PermissionCode.SystemSettings)]

// In Business Logic
_currentUser.EnsureHasAnyPermission(
    PermissionCode.UserManage,
    PermissionCode.SystemSettings);
```

### Pattern 4: AND Permission Logic

#### Before
```csharp
var hasUserManage = user.Permissions.Any(p => p.Code == "USER_MANAGE");
var hasSystemSettings = user.Permissions.Any(p => p.Code == "SYSTEM_SETTINGS");

if (!hasUserManage || !hasSystemSettings)
{
    throw new UnauthorizedAccessException();
}
```

#### After
```csharp
// In Controller
[MustHavePermission(
    PermissionCode.UserManage, 
    PermissionCode.SystemSettings, 
    RequireAll = true)]

// In Business Logic
_currentUser.EnsureHasAllPermissions(
    PermissionCode.UserManage,
    PermissionCode.SystemSettings);
```

### Pattern 5: Conditional Permissions

#### Before
```csharp
public async Task<List<Project>> GetProjects()
{
    var user = await _currentUserService.GetCurrentUserAsync();
    
    if (user.Role.Code == "ADMIN")
    {
        return await _context.Projects.ToListAsync();
    }
    else
    {
        return await _context.Projects
            .Where(p => p.Members.Any(m => m.UserId == user.Id))
            .ToListAsync();
    }
}
```

#### After
```csharp
public async Task<List<Project>> GetProjects()
{
    if (_currentUser.CanViewAllProjects())
    {
        return await _context.Projects.ToListAsync();
    }
    else
    {
        var userId = _currentUser.GetUserId();
        return await _context.Projects
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }
}
```

## Bước 5: Update Existing Controllers

### User Controller

#### Before
```csharp
public class UserController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        // Anyone can access
    }
    
    [HttpPost]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        // Public endpoint
    }
    
    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var user = await GetCurrentUser();
        if (user.Role.Code != "ADMIN")
            return Forbid();
        // ...
    }
}
```

#### After
```csharp
using IssueTracker.Domain.Entities.Enum;
using IssueTracker.WebApi.Attributes;

public class UserController : BaseApiController
{
    [HttpGet("profile")]
    [MustBeAuthenticated]  // Only require authentication
    public async Task<IActionResult> GetProfile()
    {
        // No manual check needed
    }
    
    [HttpPost("register")]
    // No attribute = public endpoint
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        // Public endpoint
    }
    
    [HttpGet("all")]
    [MustHavePermission(PermissionCode.UserManage)]  // Permission-based
    public async Task<IActionResult> GetAllUsers()
    {
        // No manual check needed
    }
}
```

### Admin Controller

#### Before
```csharp
public class AdminController : BaseApiController
{
    [HttpPost("project-roles")]
    public async Task<IActionResult> CreateProjectRole(AddProjectRoleCommand command)
    {
        var user = await GetCurrentUser();
        if (user.Role.Code != "ADMIN")
            return Forbid();
            
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
```

#### After
```csharp
using IssueTracker.Domain.Entities.Enum;
using IssueTracker.WebApi.Attributes;

[MustHaveRole(RoleCode.Admin)]  // All methods require Admin role
public class AdminController : BaseApiController
{
    [HttpPost("project-roles")]
    [MustHavePermission(PermissionCode.SystemSettings)]
    public async Task<IActionResult> CreateProjectRole(AddProjectRoleCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
```

## Bước 6: Testing Migration

### Test Cases

1. **Unauthenticated Request**
   - Expected: 401 Unauthorized
   - Message: "User is not authenticated"

2. **Authenticated but No Permission**
   - Expected: 403 Forbidden
   - Message: "User does not have required permission(s)"

3. **Authenticated with Permission**
   - Expected: 200 OK
   - Normal response

### Testing Steps

1. Test without token
```bash
curl -X GET http://localhost:5000/api/admin/users
# Expected: 401 Unauthorized
```

2. Test with user token (no admin permission)
```bash
curl -X GET http://localhost:5000/api/admin/users \
  -H "Authorization: Bearer <user_token>"
# Expected: 403 Forbidden
```

3. Test with admin token
```bash
curl -X GET http://localhost:5000/api/admin/users \
  -H "Authorization: Bearer <admin_token>"
# Expected: 200 OK
```

## Bước 7: Checklist

### Pre-Migration
- [ ] Backup current code
- [ ] Create all new attribute and filter files
- [ ] Create enum files for constants
- [ ] Update ICurrentUser implementation
- [ ] Test authentication flow still works

### During Migration
- [ ] Identify all controllers that need authorization
- [ ] Replace manual permission checks with attributes
- [ ] Update business logic to use extensions
- [ ] Replace hardcoded strings with constants
- [ ] Add appropriate attributes to each endpoint

### Post-Migration
- [ ] Test all endpoints with Swagger
- [ ] Verify 401/403 responses are correct
- [ ] Test with different user roles
- [ ] Update API documentation
- [ ] Remove old authorization code
- [ ] Run integration tests

## Common Issues & Solutions

### Issue 1: Attribute Not Working

**Problem:** Attribute is applied but still allows unauthorized access

**Solution:**
- Ensure JWT authentication is configured in Program.cs
- Check ICurrentUser is registered in DI
- Verify token contains correct claims

### Issue 2: Always Getting 401

**Problem:** Even with valid token, getting 401

**Solution:**
- Check token format: `Authorization: Bearer <token>`
- Verify token is not expired
- Check JWT configuration matches token issuer

### Issue 3: Always Getting 403

**Problem:** User has permission but still getting 403

**Solution:**
- Check permission codes match exactly
- Verify user's role has the permission in database
- Re-login to refresh token with new permissions

### Issue 4: Can't Test in Swagger

**Problem:** Swagger not sending Authorization header

**Solution:**
- Ensure Swagger is configured with JWT bearer authentication
- Click "Authorize" button in Swagger UI
- Enter token in format: `Bearer <token>` or just `<token>`

## Next Steps

After migration:
1. Remove old authorization code
2. Update documentation
3. Train team on new system
4. Add more specific permissions as needed
5. Consider implementing project-level permissions

## Support

For more details, refer to:
- `AUTHORIZATION_GUIDE.md` - Full usage guide
- `AuthorizationExampleController.cs` - Code examples
- `PermissionExtensions.cs` - Extension methods
