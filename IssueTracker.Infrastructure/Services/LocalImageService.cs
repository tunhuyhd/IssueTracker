using IssueTracker.Application.Common.Dto;
using IssueTracker.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Infrastructure.Services;

public class LocalStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(
        IWebHostEnvironment env,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LocalStorageService> logger)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        IEnumerable<FileType> allowedFileTypes,
        string subFolder,
        bool isOverwrite = false,
        CancellationToken cancellationToken = default)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File stream is null or empty");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty");

        // Convert stream to bytes for validation
        byte[] fileBytes;
        using (var memoryStream = new MemoryStream())
        {
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            fileBytes = memoryStream.ToArray();
        }

        // Validate file type using magic bytes
        BaseFileType fileType;
        try
        {
            fileType = FileTypeFactory.CreateFileTypeInstance(fileBytes);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid file format for {FileName}: {Error}", fileName, ex.Message);
            throw new InvalidOperationException("File format is invalid");
        }

        // Check if file type is allowed
        if (!allowedFileTypes.Contains(fileType.Type))
        {
            throw new InvalidOperationException($"File type {fileType.Type} is not allowed");
        }

        // Validate file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!fileType.AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File extension {extension} is not allowed for {fileType.Type} files");
        }

        // Check file size
        if (fileBytes.Length > fileType.MaxSizeInBytes)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {fileType.MaxSizeInBytes / (1024 * 1024)} MB");
        }

        // Create folder structure: files/{subFolder}/{fileType}/{year}/{month}
        var dateUtc = DateTime.UtcNow;
        var folderName = $"files/{subFolder}/{fileType.Folder}/{dateUtc.Year:D4}/{dateUtc.Month:D2}";
        var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", folderName.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(uploadsPath);

        // Create unique filename
        var cleanFileName = Path.GetFileNameWithoutExtension(fileName)
            .Trim('"')
            .Replace(' ', '_')
            .Replace("'", "")
            .Replace('"', '_');

        var uniqueFileName = $"{cleanFileName}_{dateUtc:yyyyMMddHHmmss}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(uploadsPath, uniqueFileName);

        // Check if file already exists
        if (!isOverwrite && File.Exists(filePath))
        {
            uniqueFileName = $"{cleanFileName}_{dateUtc:yyyyMMddHHmmssffff}{Path.GetExtension(fileName)}";
            filePath = Path.Combine(uploadsPath, uniqueFileName);
        }

        // Save file
        await using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
        {
            await fileStreamWriter.WriteAsync(fileBytes, cancellationToken);
        }

        // Return relative path for database storage
        var relativePath = $"{folderName}/{uniqueFileName}".Replace(Path.DirectorySeparatorChar, '/');

        _logger.LogInformation("File uploaded successfully to local storage: {Path}", relativePath);
        return relativePath.ToLowerInvariant();
    }

    public async Task<string> UploadFileAsync(
        FileUploadRequest fileUploadRequest,
        IEnumerable<FileType> allowedFileTypes,
        string subFolder,
        bool isOverwrite = false,
        CancellationToken cancellationToken = default)
    {
        await using var stream = fileUploadRequest.File.OpenReadStream();
        return await UploadFileAsync(stream, fileUploadRequest.File.FileName, allowedFileTypes, subFolder, isOverwrite, cancellationToken);
    }

    public async Task<string> GetFileAsync(string filePath)
    {
        var physicalPath = GetPhysicalPath(filePath);
        if (!File.Exists(physicalPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return await File.ReadAllTextAsync(physicalPath);
    }

    public async Task<Stream> GetFileStreamAsync(string filePath)
    {
        var physicalPath = GetPhysicalPath(filePath);
        if (!File.Exists(physicalPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var fileBytes = await File.ReadAllBytesAsync(physicalPath);
        return new MemoryStream(fileBytes);
    }

    public string GetFileUri(string filePath)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null) return filePath;

        var baseUrl = $"{request.Scheme}://{request.Host.Value}";
        return $"{baseUrl}/{filePath.TrimStart('/').Replace('\\', '/')}";
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var physicalPath = GetPhysicalPath(filePath);
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                _logger.LogInformation("File deleted from local storage: {Path}", filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from local storage: {Path}", filePath);
            return false;
        }
    }

    public string? GetThumbnailUrl(string? originalUrl, int width, int height)
    {
        // Local storage doesn't support dynamic resizing
        // Local storage doesn't support dynamic resizing
        // Return absolute URI so clients can fetch the file
        if (string.IsNullOrWhiteSpace(originalUrl)) return null;
        try
        {
            return GetFileUri(originalUrl);
        }
        catch
        {
            return originalUrl;
        }
    }

    private string GetPhysicalPath(string relativePath)
    {
        // Convert relative path to physical path
        var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        return Path.Combine(_env.WebRootPath ?? "wwwroot", normalizedPath);
    }
}
