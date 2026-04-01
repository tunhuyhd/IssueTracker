using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using IssueTracker.Application.Common.Dto;
using IssueTracker.Application.Common.Interfaces;
using IssueTracker.Infrastructure.Services.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IssueTracker.Infrastructure.Services;

public class CloudinaryStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryStorageService> _logger;

    public CloudinaryStorageService(
        IOptions<CloudinarySettings> cloudinaryOptions,
        ILogger<CloudinaryStorageService> logger)
    {
        _settings = cloudinaryOptions.Value;
        _logger = logger;

        var account = new Account(
            _settings.CloudName,
            _settings.ApiKey,
            _settings.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = _settings.UseSecureUrl } };
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

        // Create unique filename
        var uniqueFileName = Path.GetFileNameWithoutExtension(fileName)
            .Trim('"')
            .Replace(' ', '_')
            .Replace("'", "")
            .Replace('"', '_')
            + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        // Create public ID (without extension)
        var publicId = $"{_settings.RootFolder}/{subFolder}/{fileType.Folder}/{uniqueFileName}";

        // Upload to Cloudinary
        var uploadResult = await UploadToCloudinaryAsync(fileBytes, fileName, publicId, fileType.Type, cancellationToken);

        if (uploadResult.StatusCode != HttpStatusCode.OK && uploadResult.StatusCode != HttpStatusCode.Created)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
            throw new InvalidOperationException("Image upload failed");
        }

        _logger.LogInformation("File uploaded successfully to Cloudinary: {PublicId}", uploadResult.PublicId);
        return uploadResult.SecureUrl.ToString();
    }

    public async Task<string> UploadFileAsync(
        FileUploadRequest fileUploadRequest,
        IEnumerable<FileType> allowedFileTypes,
        string subFolder,
        bool isOverwrite = false,
        CancellationToken cancellationToken = default)
    {
        using var stream = fileUploadRequest.File.OpenReadStream();
        return await UploadFileAsync(stream, fileUploadRequest.File.FileName, allowedFileTypes, subFolder, isOverwrite, cancellationToken);
    }

    public async Task<string> GetFileAsync(string filePath)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(filePath);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Stream> GetFileStreamAsync(string filePath)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(filePath);
        response.EnsureSuccessStatusCode();
        var memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public string GetFileUri(string filePath) => filePath;

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var publicId = ExtractPublicIdFromUrl(filePath);
            if (string.IsNullOrWhiteSpace(publicId)) return false;

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            _logger.LogInformation("File deleted from Cloudinary: {PublicId}, Result: {Result}", publicId, result.Result);
            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Cloudinary: {FilePath}", filePath);
            return false;
        }
    }

    public string? GetThumbnailUrl(string? originalUrl, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(originalUrl) || !originalUrl.Contains("cloudinary.com"))
            return originalUrl;

        // Insert transformation parameters into Cloudinary URL
        // From: https://res.cloudinary.com/.../image/upload/v123/path.jpg
        // To:   https://res.cloudinary.com/.../image/upload/w_300,h_300,c_fill/v123/path.jpg
        return originalUrl.Replace("/upload/", $"/upload/w_{width},h_{height},c_fill/");
    }

    private async Task<UploadResult> UploadToCloudinaryAsync(
        byte[] fileBytes,
        string fileName,
        string publicId,
        FileType fileType,
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(fileBytes);

        // Add image-specific transformations if file is an image
        if (fileType == FileType.Image)
        {
            var imageUploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                PublicId = publicId,
                Overwrite = false,
                UniqueFilename = false,
                UseFilename = true,
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
            };
            return await _cloudinary.UploadAsync(imageUploadParams);
        }
        else
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, stream),
                PublicId = publicId,
                Overwrite = false,
                UniqueFilename = false,
                UseFilename = true
            };
            return await _cloudinary.UploadAsync(uploadParams);
        }
    }

    private static ResourceType GetCloudinaryResourceType(FileType fileType)
    {
        return fileType switch
        {
            FileType.Image => ResourceType.Image,
            FileType.Video => ResourceType.Video,
            FileType.Audio => ResourceType.Video, // Audio files use Video resource type
            _ => ResourceType.Raw
        };
    }

    private string ExtractPublicIdFromUrl(string url)
    {
        try
        {
            // Extract public ID from Cloudinary URL
            // URL format: https://res.cloudinary.com/{cloud_name}/{resource_type}/upload/v{version}/{public_id}.{format}
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');

            // Find the "upload" segment and extract everything after the version
            var uploadIndex = Array.IndexOf(segments, "upload");
            if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length) return string.Empty;

            // Skip "upload" and version (v1234567890), then join the remaining segments
            var pathAfterVersion = segments[(uploadIndex + 2)..];
            var publicIdWithExtension = string.Join("/", pathAfterVersion);

            // Remove file extension
            return Path.GetFileNameWithoutExtension(publicIdWithExtension);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting public ID from URL: {Url}", url);
            return string.Empty;
        }
    }
}
