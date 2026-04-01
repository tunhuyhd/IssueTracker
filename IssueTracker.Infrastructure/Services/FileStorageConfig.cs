using IssueTracker.Application.Common.Interfaces;
using IssueTracker.Infrastructure.Services.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IssueTracker.Infrastructure.Services;

public static class FileStorageConfig
{
    /// <summary>
    /// Thêm Cloudinary service với configuration
    /// </summary>
    public static IServiceCollection AddCloudinaryService(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<CloudinarySettings>(options =>
            config.GetSection("FileStorage:Cloudinary").Bind(options));

        return services;
    }

    /// <summary>
    /// Thêm file storage services với automatic provider selection
    /// </summary>
    public static IServiceCollection AddFileStorageServices(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<CloudinaryStorageService>>();

        var cloudinarySettings = serviceProvider.GetService<IOptions<CloudinarySettings>>()?.Value;

        // Ưu tiên 1: Cloudinary (nếu có config đầy đủ)
        if (cloudinarySettings.IsCloudinaryConfigured())
        {
            logger?.LogInformation("Using CloudinaryStorageService");
            services.AddScoped<IFileStorageService, CloudinaryStorageService>();
        }
        // Fallback: Local Storage (không cần config)
        else
        {
            logger?.LogInformation("Using LocalStorageService (fallback)");
            services.AddScoped<IFileStorageService, LocalStorageService>();
        }

        return services;
    }
}