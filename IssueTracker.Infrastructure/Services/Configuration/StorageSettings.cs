namespace IssueTracker.Infrastructure.Services.Configuration;

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string RootFolder { get; set; } = "issuetracker";
    public bool UseSecureUrl { get; set; } = true;
}

public class FileStorageSettings
{
    public CloudinarySettings? Cloudinary { get; set; }
    public string DefaultProvider { get; set; } = "local"; // local, cloudinary
}

public static class FileStorageExtensions
{
    public static bool IsCloudinaryConfigured(this CloudinarySettings? settings)
    {
        return settings != null &&
               !string.IsNullOrWhiteSpace(settings.CloudName) &&
               !string.IsNullOrWhiteSpace(settings.ApiKey) &&
               !string.IsNullOrWhiteSpace(settings.ApiSecret);
    }
}