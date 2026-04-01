using IssueTracker.Application.Common.Dto;

namespace IssueTracker.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Upload file từ Stream
    /// </summary>
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        IEnumerable<FileType> allowedFileTypes,
        string subFolder,
        bool isOverwrite = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload file từ IFormFile
    /// </summary>
    Task<string> UploadFileAsync(
        FileUploadRequest fileUploadRequest,
        IEnumerable<FileType> allowedFileTypes,
        string subFolder,
        bool isOverwrite = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy nội dung file dạng text
    /// </summary>
    Task<string> GetFileAsync(string filePath);

    /// <summary>
    /// Lấy file dạng Stream
    /// </summary>
    Task<Stream> GetFileStreamAsync(string filePath);

    /// <summary>
    /// Lấy URI của file
    /// </summary>
    string GetFileUri(string filePath);

    /// <summary>
    /// Delete file
    /// </summary>
    Task<bool> DeleteFileAsync(string filePath);

    /// <summary>
    /// Tạo thumbnail URL (chỉ cho Cloudinary)
    /// </summary>
    string? GetThumbnailUrl(string? originalUrl, int width, int height);
}