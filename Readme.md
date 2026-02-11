# Hướng Dẫn Xây Dựng Clean Architecture với CQRS Pattern

## 📋 Mục Lục
1. [Tổng Quan Kiến Trúc](#tổng-quan-kiến-trúc)
2. [Cấu Trúc Solution](#cấu-trúc-solution)
3. [Hướng Dẫn Thiết Lập Từng Layer](#hướng-dẫn-thiết-lập-từng-layer)
4. [Implement CQRS Pattern](#implement-cqrs-pattern)
5. [Dependency Injection](#dependency-injection)
6. [Best Practices](#best-practices)

---

## Tổng Quan Kiến Trúc

### Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│         (WebApi/Controllers)            │
├─────────────────────────────────────────┤
│         Application Layer               │
│    (CQRS: Commands & Queries)           │
├─────────────────────────────────────────┤
│         Infrastructure Layer            │
│  (DbContext, Repositories, Services)    │
├─────────────────────────────────────────┤
│         Domain Layer                    │
│    (Entities, Interfaces, Events)       │
└─────────────────────────────────────────┘
```

### Dependency Flow
- **WebApi** → **Application** → **Infrastructure** → **Domain**
- Domain không phụ thuộc vào layer nào khác
- Application chỉ phụ thuộc vào Domain
- Infrastructure implement các interface từ Application và Domain

---

## Cấu Trúc Solution

### 1. Tạo Solution và Projects

```bash
# Tạo solution
dotnet new sln -n YourProjectName

# Tạo các projects
dotnet new classlib -n YourProjectName.Domain
dotnet new classlib -n YourProjectName.Application
dotnet new classlib -n YourProjectName.Infrastructure
dotnet new webapi -n YourProjectName.WebApi
dotnet new xunit -n YourProjectName.Infrastructure.Test

# Thêm projects vào solution
dotnet sln add YourProjectName.Domain/YourProjectName.Domain.csproj
dotnet sln add YourProjectName.Application/YourProjectName.Application.csproj
dotnet sln add YourProjectName.Infrastructure/YourProjectName.Infrastructure.csproj
dotnet sln add YourProjectName.WebApi/YourProjectName.WebApi.csproj
dotnet sln add YourProjectName.Infrastructure.Test/YourProjectName.Infrastructure.Test.csproj

# Thiết lập dependencies
dotnet add YourProjectName.Application reference YourProjectName.Domain
dotnet add YourProjectName.Infrastructure reference YourProjectName.Application
dotnet add YourProjectName.Infrastructure reference YourProjectName.Domain
dotnet add YourProjectName.WebApi reference YourProjectName.Application
dotnet add YourProjectName.WebApi reference YourProjectName.Infrastructure
dotnet add YourProjectName.Infrastructure.Test reference YourProjectName.Infrastructure
```

---

## Hướng Dẫn Thiết Lập Từng Layer

## 1. Domain Layer

**Mục đích:** Chứa business logic core, entities, và các interface không phụ thuộc vào bất kỳ framework nào.

### Cấu trúc thư mục:
```
Domain/
├── Common/
│   ├── Entity.cs
│   ├── AuditableEntity.cs
│   ├── IAuditableEntity.cs
│   ├── IAggregateRoot.cs
│   ├── IRepository.cs
│   ├── IReadRepository.cs
│   └── BaseEvent.cs
├── Entities/
│   ├── User.cs
│   ├── Company.cs
│   ├── Client.cs
│   └── ...
└── Events/
    └── ClientCreatedEvent.cs
```

### 1.1. Base Entity Class

**File: `Domain/Common/Entity.cs`**
```csharp
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProjectName.Domain.Common;

public abstract class Entity
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    private readonly List<BaseEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

### 1.2. Auditable Entity

**File: `Domain/Common/IAuditableEntity.cs`**
```csharp
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProjectName.Domain.Common;

public interface IAuditableEntity
{
    Guid CreatedBy { get; set; }
    DateTime CreatedOn { get; }
    Guid LastModifiedBy { get; set; }
    DateTime? LastModifiedOn { get; set; }
}

public interface ISoftDelete
{
    DateTime? DeletedOn { get; set; }
    Guid? DeletedBy { get; set; }
}

public abstract class AuditableEntity : Entity, IAuditableEntity, ISoftDelete
{
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;

    [Column("last_modified_by")]
    public Guid LastModifiedBy { get; set; }

    [Column("last_modified_on")]
    public DateTime? LastModifiedOn { get; set; }

    [Column("deleted_on")]
    public DateTime? DeletedOn { get; set; }

    [Column("deleted_by")]
    public Guid? DeletedBy { get; set; }
}
```

### 1.3. Aggregate Root Marker

**File: `Domain/Common/IAggregateRoot.cs`**
```csharp
namespace YourProjectName.Domain.Common;

// Marker interface for aggregate roots
public interface IAggregateRoot
{
}
```

### 1.4. Repository Interfaces

**File: `Domain/Common/IRepository.cs`**
```csharp
namespace YourProjectName.Domain.Common;

public interface IRepository<T> : IReadRepository<T> where T : class, IAggregateRoot
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}
```

**File: `Domain/Common/IReadRepository.cs`**
```csharp
using System.Linq.Expressions;

namespace YourProjectName.Domain.Common;

public interface IReadRepository<T> where T : class, IAggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T?> GetOneAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
```

### 1.5. Domain Events

**File: `Domain/Common/BaseEvent.cs`**
```csharp
using MediatR;

namespace YourProjectName.Domain.Common;

public abstract class BaseEvent : INotification
{
    public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
}
```

### 1.6. Sample Entity

**File: `Domain/Entities/Client.cs`**
```csharp
using System.ComponentModel.DataAnnotations.Schema;
using YourProjectName.Domain.Common;

namespace YourProjectName.Domain.Entities;

[Table("clients")]
public class Client : AuditableEntity, IAggregateRoot
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("user")]
    public required User User { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("company_id")]
    public Guid? CompanyId { get; set; }

    public Company? Company { get; set; }
}
```

---

## 2. Application Layer

**Mục đích:** Chứa business logic, CQRS commands/queries, DTOs, validators, và các interface cho services.

### Packages cần thiết:
```xml
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
```

### Cấu trúc thư mục:
```
Application/
├── Common/
│   ├── IApplicationDbContext.cs
│   ├── ICurrentUser.cs
│   ├── Dto/
│   │   └── Pagination.cs
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs
│   ├── Exceptions/
│   │   ├── ValidationException.cs
│   │   └── NotFoundException.cs
│   └── Authorization/
│       └── ICurrentUser.cs
├── Clients/
│   ├── Commands/
│   │   ├── CreateClientCommand.cs
│   │   └── UpdateClientCommand.cs
│   ├── Queries/
│   │   ├── GetClientQuery.cs
│   │   └── GetClientsQuery.cs
│   └── Repositories/
│       └── IClientRepository.cs
└── Startup.cs
```

### 2.1. DbContext Interface

**File: `Application/Common/IApplicationDbContext.cs`**
```csharp
using Microsoft.EntityFrameworkCore;
using YourProjectName.Domain.Entities;

namespace YourProjectName.Application.Common;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Company> Companies { get; }
    DbSet<Client> Clients { get; }
    // Add other DbSets...

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### 2.2. Current User Service Interface

**File: `Application/Common/Authorization/ICurrentUser.cs`**
```csharp
namespace YourProjectName.Application.Common.Authorization;

public interface ICurrentUser
{
    Guid GetUserId();
    Guid? GetCompanyId();
    string? GetPhoneNumber();
    string? GetDisplayedName();
    List<string> GetRoles();
}
```

### 2.3. CQRS Command Example

**File: `Application/Clients/Commands/CreateClientCommand.cs`**
```csharp
using MediatR;
using YourProjectName.Application.Common;
using YourProjectName.Application.Common.Authorization;
using YourProjectName.Domain.Common;
using YourProjectName.Domain.Entities;

namespace YourProjectName.Application.Clients.Commands;

// Command
public class CreateClientCommand : IRequest<Guid>
{
    public string Phone { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}

// Command Handler
public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<Client> _clientRepository;

    public CreateClientCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IRepository<Client> clientRepository)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _clientRepository = clientRepository;
    }

    public async Task<Guid> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var companyId = _currentUser.GetCompanyId();

        var client = new Client
        {
            UserId = userId,
            Phone = request.Phone,
            Name = request.Name,
            Email = request.Email,
            CompanyId = companyId,
            User = null! // Will be set by EF Core
        };

        await _clientRepository.AddAsync(client, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return client.Id;
    }
}

// Validator (Optional but recommended)
public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^\d{9,15}$").WithMessage("Invalid phone format");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}
```

### 2.4. CQRS Query Example

**File: `Application/Clients/Queries/GetClientQuery.cs`**
```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using YourProjectName.Application.Common;
using YourProjectName.Application.Common.Exceptions;

namespace YourProjectName.Application.Clients.Queries;

// Query
public record GetClientQuery(Guid ClientId) : IRequest<ClientDto>;

// DTO
public class ClientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}

// Query Handler
public class GetClientQueryHandler : IRequestHandler<GetClientQuery, ClientDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetClientQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClientDto> Handle(GetClientQuery request, CancellationToken cancellationToken)
    {
        var client = await _dbContext.Clients
            .Where(c => c.Id == request.ClientId)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                Name = c.Name ?? string.Empty,
                Phone = c.Phone ?? string.Empty,
                Email = c.Email
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (client == null)
            throw new NotFoundException($"Client with ID {request.ClientId} not found");

        return client;
    }
}
```

### 2.5. Pagination Helper

**File: `Application/Common/Dto/Pagination.cs`**
```csharp
namespace YourProjectName.Application.Common.Dto;

public class Pagination<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

### 2.6. Custom Exceptions

**File: `Application/Common/Exceptions/NotFoundException.cs`**
```csharp
namespace YourProjectName.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
```

**File: `Application/Common/Exceptions/ValidationException.cs`**
```csharp
namespace YourProjectName.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(IEnumerable<string> errors) 
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IEnumerable<string> Errors { get; }
}
```

### 2.7. Validation Pipeline Behavior

**File: `Application/Common/Behaviors/ValidationBehavior.cs`**
```csharp
using FluentValidation;
using MediatR;

namespace YourProjectName.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            throw new Exceptions.ValidationException(
                failures.Select(f => f.ErrorMessage));
        }

        return await next();
    }
}
```

### 2.8. Application Startup Configuration

**File: `Application/Startup.cs`**
```csharp
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using YourProjectName.Application.Common.Behaviors;

namespace YourProjectName.Application;

public static class Startup
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(assembly));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), 
            typeof(ValidationBehavior<,>));

        return services;
    }
}
```

---

## 3. Infrastructure Layer

**Mục đích:** Implementation của các interfaces từ Application layer, database context, repositories, external services.

### Packages cần thiết:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<!-- OR -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
```

### Cấu trúc thư mục:
```
Infrastructure/
├── Persistence/
│   ├── Context/
│   │   ├── ApplicationDbContext.cs
│   │   └── BaseDbContext.cs
│   ├── Configuration/
│   │   └── ClientConfiguration.cs
│   ├── Repositories/
│   │   ├── ApplicationDbRepository.cs
│   │   └── ClientRepository.cs
│   ├── Interceptors/
│   │   └── DispatchDomainEventsInterceptor.cs
│   ├── Initialization/
│   │   └── DatabaseInitializer.cs
│   └── Startup.cs
├── Auth/
│   ├── CurrentUser.cs
│   └── Startup.cs
└── Startup.cs
```

### 3.1. DbContext Implementation

**File: `Infrastructure/Persistence/Context/ApplicationDbContext.cs`**
```csharp
using Microsoft.EntityFrameworkCore;
using YourProjectName.Application.Common;
using YourProjectName.Application.Common.Authorization;
using YourProjectName.Domain.Entities;

namespace YourProjectName.Infrastructure.Persistence.Context;

public class ApplicationDbContext : BaseDbContext, IApplicationDbContext
{
    public ApplicationDbContext(
        DbContextOptions options,
        ICurrentUser currentUser) 
        : base(options, currentUser)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Client> Clients => Set<Client>();
    // Add other DbSets...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());

        // Set default schema if needed
        modelBuilder.HasDefaultSchema("your_schema");
    }
}
```

**File: `Infrastructure/Persistence/Context/BaseDbContext.cs`**
```csharp
using Microsoft.EntityFrameworkCore;
using YourProjectName.Application.Common.Authorization;
using YourProjectName.Domain.Common;

namespace YourProjectName.Infrastructure.Persistence.Context;

public abstract class BaseDbContext : DbContext
{
    private readonly ICurrentUser _currentUser;

    protected BaseDbContext(
        DbContextOptions options,
        ICurrentUser currentUser) 
        : base(options)
    {
        _currentUser = currentUser;
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var userId = _currentUser.GetUserId();
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = userId;
                    // CreatedOn is set in constructor
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = userId;
                    entry.Entity.LastModifiedOn = now;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.DeletedOn = now;
                entry.Entity.DeletedBy = userId;
            }
        }
    }
}
```

### 3.2. Repository Implementation

**File: `Infrastructure/Persistence/Repositories/ApplicationDbRepository.cs`**
```csharp
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YourProjectName.Domain.Common;
using YourProjectName.Infrastructure.Persistence.Context;

namespace YourProjectName.Infrastructure.Persistence.Repositories;

public class ApplicationDbRepository<T> : IRepository<T> 
    where T : class, IAggregateRoot
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public ApplicationDbRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities, 
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(
        IEnumerable<T> entities, 
        CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<T?> GetOneAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<List<T>> ListAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }
}
```

### 3.3. Domain Events Interceptor

**File: `Infrastructure/Persistence/Interceptors/DispatchDomainEventsInterceptor.cs`**
```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using YourProjectName.Domain.Common;

namespace YourProjectName.Infrastructure.Persistence.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;

    public DispatchDomainEventsInterceptor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEvents(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEvents(
        DbContext context, 
        CancellationToken cancellationToken)
    {
        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
```

### 3.4. Current User Implementation

**File: `Infrastructure/Auth/CurrentUser.cs`**
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using YourProjectName.Application.Common.Authorization;

namespace YourProjectName.Infrastructure.Auth;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId) 
            ? userId 
            : Guid.Empty;
    }

    public Guid? GetCompanyId()
    {
        var companyIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst("CompanyId")?.Value;

        return Guid.TryParse(companyIdClaim, out var companyId) 
            ? companyId 
            : null;
    }

    public string? GetPhoneNumber()
    {
        return _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.MobilePhone)?.Value;
    }

    public string? GetDisplayedName()
    {
        return _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Name)?.Value;
    }

    public List<string> GetRoles()
    {
        return _httpContextAccessor.HttpContext?.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();
    }
}
```

### 3.5. Infrastructure Startup Configuration

**File: `Infrastructure/Startup.cs`**
```csharp
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YourProjectName.Application.Common;
using YourProjectName.Application.Common.Authorization;
using YourProjectName.Domain.Common;
using YourProjectName.Infrastructure.Auth;
using YourProjectName.Infrastructure.Persistence.Context;
using YourProjectName.Infrastructure.Persistence.Interceptors;
using YourProjectName.Infrastructure.Persistence.Repositories;

namespace YourProjectName.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        return services
            .AddHttpContextAccessor()
            .AddAuth()
            .AddPersistence(configuration);
    }

    private static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Register interceptors
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        // Register DbContext
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            
            // For SQL Server:
            options.UseSqlServer(connectionString, 
                b => b.MigrationsAssembly(Assembly.GetExecutingAssembly().ToString()));
            
            // For PostgreSQL:
            // options.UseNpgsql(connectionString, 
            //     b => b.MigrationsAssembly(Assembly.GetExecutingAssembly().ToString()));
        });

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(ApplicationDbRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(ApplicationDbRepository<>));

        return services;
    }
}
```

---

## 4. WebApi Layer (Presentation)

### Packages cần thiết:
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

### Cấu trúc thư mục:
```
WebApi/
├── Controllers/
│   ├── BaseApiController.cs
│   └── ClientsController.cs
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── Startup.cs (optional)
```

### 4.1. Base API Controller

**File: `WebApi/Controllers/BaseApiController.cs`**
```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace YourProjectName.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => 
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
```

### 4.2. Sample Controller

**File: `WebApi/Controllers/ClientsController.cs`**
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourProjectName.Application.Clients.Commands;
using YourProjectName.Application.Clients.Queries;

namespace YourProjectName.WebApi.Controllers;

[Authorize]
public class ClientsController : BaseApiController
{
    [HttpGet("{clientId:guid}")]
    public async Task<IActionResult> GetClient(
        Guid clientId, 
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetClientQuery(clientId), 
            cancellationToken);
        
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetClients(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetClientsQuery 
        { 
            Page = page, 
            PageSize = pageSize 
        };
        
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient(
        [FromBody] CreateClientCommand command,
        CancellationToken cancellationToken)
    {
        var clientId = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetClient), 
            new { clientId }, 
            new { id = clientId });
    }

    [HttpPut("{clientId:guid}")]
    public async Task<IActionResult> UpdateClient(
        Guid clientId,
        [FromBody] UpdateClientCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = clientId;
        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{clientId:guid}")]
    public async Task<IActionResult> DeleteClient(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(
            new DeleteClientCommand { Id = clientId }, 
            cancellationToken);
        
        return NoContent();
    }
}
```

### 4.3. Program.cs

**File: `WebApi/Program.cs`**
```csharp
using YourProjectName.Application;
using YourProjectName.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Your API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Application and Infrastructure layers
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 4.4. appsettings.json

**File: `WebApi/appsettings.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=YourDb;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## Implement CQRS Pattern

### CQRS Flow Diagram

```
┌──────────────┐
│  Controller  │
└──────┬───────┘
       │
       ▼
┌──────────────┐      ┌─────────────┐
│   MediatR    │─────▶│  Validator  │
└──────┬───────┘      └─────────────┘
       │
       ├─────────────────┬─────────────────┐
       ▼                 ▼                 ▼
┌────────────┐    ┌────────────┐    ┌────────────┐
│  Commands  │    │  Queries   │    │   Events   │
│  (Write)   │    │   (Read)   │    └────────────┘
└─────┬──────┘    └─────┬──────┘
      │                 │
      ▼                 ▼
┌────────────┐    ┌────────────┐
│ Repository │    │  DbContext │
└─────┬──────┘    └─────┬──────┘
      │                 │
      └────────┬────────┘
               ▼
        ┌──────────────┐
        │   Database   │
        └──────────────┘
```

### Best Practices cho CQRS

1. **Commands (Write Operations)**
   - Thay đổi state của system
   - Return void hoặc ID của entity đã tạo
   - Luôn validate input
   - Sử dụng repository pattern
   - Example: `CreateClientCommand`, `UpdateClientCommand`

2. **Queries (Read Operations)**
   - Không thay đổi state
   - Return DTOs, không return entities trực tiếp
   - Có thể query trực tiếp từ DbContext để optimize
   - Sử dụng projection với `Select()`
   - Example: `GetClientQuery`, `GetClientsQuery`

3. **Handlers**
   - Một handler cho một command/query
   - Keep it simple and focused
   - Inject dependencies qua constructor
   - Handle business logic validation

---

## Dependency Injection

### Dependency Flow

```
Program.cs
  │
  ├─> AddApplication()
  │     ├─> MediatR
  │     ├─> FluentValidation
  │     └─> Pipeline Behaviors
  │
  └─> AddInfrastructure()
        ├─> DbContext
        ├─> Repositories
        ├─> CurrentUser
        └─> External Services
```

### Registration Order

1. **Application Layer** (đăng ký đầu tiên)
   - MediatR handlers
   - Validators
   - Pipeline behaviors

2. **Infrastructure Layer**
   - DbContext
   - Repositories
   - Services implementation

3. **WebApi Layer**
   - Controllers
   - Middleware
   - Authentication/Authorization

---

## Best Practices

### 1. Entity Design
- ✅ Sử dụng `AuditableEntity` cho các entity cần tracking
- ✅ Implement `IAggregateRoot` cho root entities
- ✅ Sử dụng Domain Events cho cross-aggregate communication
- ❌ Không reference entities khác nhau trực tiếp trong queries

### 2. CQRS Implementation
- ✅ Commands để modify data, Queries để read data
- ✅ Return DTOs from queries, không return entities
- ✅ Validate commands với FluentValidation
- ✅ Keep handlers simple và focused
- ❌ Không mix read and write logic trong cùng handler

### 3. Repository Pattern
- Repository cho aggregate roots only
- Sử dụng generic repository cho CRUD operations
- Custom repositories cho complex queries
- Không expose IQueryable từ repositories

### 4. Validation
- Input validation trong Command validators
- Business rule validation trong Handlers
- Database validation với EF Core constraints
- Không validate trong Controllers

### 5. Exception Handling
- Sử dụng custom exceptions (NotFoundException, ValidationException)
- Global exception middleware để catch và format errors
- Return appropriate HTTP status codes
- Không let exceptions propagate to client

### 6. Dependency Injection
- Register services trong `Startup.cs` của mỗi layer
- Sử dụng interfaces để loose coupling
- Scoped lifetime cho DbContext và Repositories
- Không inject DbContext trực tiếp vào Controllers

### 7. Testing
- Unit test cho Handlers với mocked dependencies
- Integration test cho Repositories với test database
- Test validators riêng biệt
- E2E test cho critical flows

---

## Migration Commands

```bash
# Add migration
dotnet ef migrations add InitialCreate --project YourProjectName.Infrastructure --startup-project YourProjectName.WebApi

# Update database
dotnet ef database update --project YourProjectName.Infrastructure --startup-project YourProjectName.WebApi

# Remove last migration
dotnet ef migrations remove --project YourProjectName.Infrastructure --startup-project YourProjectName.WebApi

# Generate SQL script
dotnet ef migrations script --project YourProjectName.Infrastructure --startup-project YourProjectName.WebApi
```

---

## Common Patterns

### 1. Soft Delete với Global Query Filter

```csharp
// In BaseDbContext.OnModelCreating
modelBuilder.Entity<Client>().HasQueryFilter(e => e.DeletedOn == null);
```

### 2. Pagination Extension

```csharp
public static class QueryableExtensions
{
    public static async Task<Pagination<T>> ToPaginationAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new Pagination<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
```

### 3. Result Pattern (Optional)

```csharp
public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    
    public static Result<T> Success(T data) => 
        new() { IsSuccess = true, Data = data };
    
    public static Result<T> Failure(string error) => 
        new() { IsSuccess = false, Error = error };
}
```

---

## Checklist Tạo Dự Án Mới

- [ ] Tạo Solution và 4 projects (Domain, Application, Infrastructure, WebApi)
- [ ] Setup dependencies giữa các projects
- [ ] Cài đặt các NuGet packages cần thiết
- [ ] Tạo base classes trong Domain (Entity, AuditableEntity)
- [ ] Tạo repository interfaces trong Domain
- [ ] Setup IApplicationDbContext trong Application
- [ ] Implement DbContext trong Infrastructure
- [ ] Implement Repository trong Infrastructure
- [ ] Setup MediatR và FluentValidation
- [ ] Tạo Pipeline Behaviors
- [ ] Implement CurrentUser service
- [ ] Setup Dependency Injection trong mỗi layer
- [ ] Tạo BaseApiController
- [ ] Configure Program.cs
- [ ] Setup appsettings.json
- [ ] Add first migration
- [ ] Test với một entity đơn giản

---

## Tài Nguyên Tham Khảo

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)

