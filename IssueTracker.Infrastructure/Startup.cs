using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Services;
using IssueTracker.Application.Identity;
using IssueTracker.Domain.Common;
using IssueTracker.Infrastructure.Auth;
using IssueTracker.Infrastructure.Persistence.Context;
using IssueTracker.Infrastructure.Persistence.Interceptors;
using IssueTracker.Infrastructure.Persistence.Repositories;
using IssueTracker.Infrastructure.Services;
using IssueTracker.Application.Common.Interfaces;
using IssueTracker.Infrastructure.Services.Configuration;

namespace IssueTracker.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddAuth();
        services.AddPersistence(configuration);

        // Configure file storage settings
        services.Configure<FileStorageSettings>(
            configuration.GetSection("FileStorage"));

        // Add file storage services with automatic provider selection
        services
            .AddCloudinaryService(configuration)
            .AddFileStorageServices();

        return services;
    }

    private static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
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

            options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name));
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(ApplicationDbRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(ApplicationDbRepository<>));

        return services;
    }
}
