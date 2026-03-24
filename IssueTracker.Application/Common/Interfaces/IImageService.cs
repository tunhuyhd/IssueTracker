using System.IO;

namespace IssueTracker.Application.Common.Interfaces
{
    public interface IImageService
    {
        Task<(string Url, string? PublicId)> UploadAsync(Stream fileStream, string fileName, string folder, string? contentType = null, CancellationToken cancellationToken = default);
        Task DeleteAsync(string? publicId, CancellationToken cancellationToken = default);
    }
}
