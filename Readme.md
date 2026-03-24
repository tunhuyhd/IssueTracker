# Issue Tracker Backend - Clean Architecture với CQRS Pattern

## Mục Lục
1. [Tổng Quan](#tổng-quan)
2. [Development Setup](#development-setup)
3. [Database Migrations](#database-migrations)
4. [CQRS Pattern](#cqrs-pattern)
5. [Best Practices](#best-practices)

---

## Tổng Quan

### Clean Architecture Layers

```text
WebApi → Application → Infrastructure → Domain
```

**Domain** không phụ thuộc vào layer nào khác  
**Application** chỉ phụ thuộc vào Domain  
**Infrastructure** implement interfaces từ Application và Domain  

### Cấu Trúc Solution

```text
IssueTracker.slnx
├── IssueTracker.Domain/           # Entities, Interfaces, Events
├── IssueTracker.Application/      # Commands & Queries (CQRS)
├── IssueTracker.Infrastructure/   # DbContext, Repositories
├── IssueTracker.WebApi/          # API Controllers
└── IssueTracker.Infrastructure.Test/ # Tests
```

---

## Development Setup

### Prerequisites
- .NET 8 SDK
- SQL Server hoặc PostgreSQL
- Visual Studio 2022 hoặc VS Code

### Quick Start

```bash
# Clone repository
git clone <repository-url>
cd IssueTracker.Backend

# Restore packages
dotnet restore

# Update database
dotnet ef database update --project IssueTracker.Infrastructure --startup-project IssueTracker.WebApi

# Run application
dotnet run --project IssueTracker.WebApi
```

---

## Database Migrations

### Tạo Migration
```bash
# Migration mới
dotnet ef migrations add <MigrationName> --project IssueTracker.Infrastructure --startup-project IssueTracker.WebApi --output-dir "./Persistence/Migrations"
```

### Cập Nhật Database
```bash
# Update lên latest migration
dotnet ef database update --project IssueTracker.Infrastructure --startup-project IssueTracker.WebApi

# Update đến migration cụ thể
dotnet ef database update <MigrationName> --project IssueTracker.Infrastructure --startup-project IssueTracker.WebApi
```

### Rollback Migration
```bash
# Lùi về migration trước
dotnet ef database update <PreviousMigrationName> --project IssueTracker.Infrastructure --startup-project IssueTracker.WebApi

# Lùi về ban đầu (xóa tất cả)
dotnet ef database update 0 --project IssueTracker.Infrastructure --startup-project IssueTracker.WebApi
```

### Xóa Migration
```bash
# Xóa migration cuối cùng
dotnet ef migrations remove --project IssueTracker.Infrastructure --startup-project IssueTracker.WebApi
```

---

## CQRS Pattern

### Commands (Write Operations)
- Thay đổi state của system
- Return void hoặc ID
- Validate với FluentValidation
- Sử dụng Repository pattern

### Queries (Read Operations)  
- Không thay đổi state
- Return DTOs
- Query trực tiếp từ DbContext
- Sử dụng projection với Select()

### Example Command
```csharp
public class CreateUserCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    // Implementation...
}
```

---

## Best Practices

### Entity Design
- Sử dụng `AuditableEntity` cho tracking
- Implement `IAggregateRoot` cho root entities
- Domain Events cho cross-aggregate communication

### Repository Pattern
- Repository cho aggregate roots only
- Generic repository cho CRUD operations
- Custom repositories cho complex queries

### Validation
- Input validation trong Command validators
- Business validation trong Handlers
- Database validation với EF Core constraints

### Exception Handling
- Custom exceptions (NotFoundException, ValidationException)
- Global exception middleware
- Return appropriate HTTP status codes

---

## Tài Nguyên Tham Khảo

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)

