using Microsoft.AspNetCore.Http;

namespace IssueTracker.Application.Common.Dto;

/// <summary>
/// DTO cho việc upload file
/// </summary>
public class FileUploadRequest
{
    /// <summary>
    /// File được upload từ IFormFile
    /// </summary>
    public IFormFile? File { get; set; }

    /// <summary>
    /// Tên file (optional, nếu không có sẽ dùng tên gốc)
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Metadata bổ sung cho file
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
