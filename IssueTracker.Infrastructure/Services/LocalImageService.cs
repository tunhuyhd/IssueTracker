using IssueTracker.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace IssueTracker.Infrastructure.Services
{
    public class LocalImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalImageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task DeleteAsync(string? publicId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return;
            var path = Path.Combine(_env.WebRootPath ?? "wwwroot", publicId.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            await Task.CompletedTask;
        }

        public async Task<(string Url, string? PublicId)> UploadAsync(Stream fileStream, string fileName, string folder, string? contentType = null, CancellationToken cancellationToken = default)
        {
            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", folder);
            Directory.CreateDirectory(uploads);

            var ext = Path.GetExtension(fileName);
            var safeFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, safeFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream, cancellationToken);
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request == null ? string.Empty : $"{request.Scheme}://{request.Host.Value}";
            var url = $"{baseUrl}/uploads/{folder}/{safeFileName}";
            var publicId = Path.Combine("uploads", folder, safeFileName).Replace(Path.DirectorySeparatorChar, '/');
            return (url, publicId);
        }
    }
}
