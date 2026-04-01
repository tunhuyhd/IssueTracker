using Microsoft.AspNetCore.Http;

namespace IssueTracker.WebApi.Models;

// DTO for multipart/form-data upload used by Swagger and model binding
public class UpdateProfileRequest
{
    public IFormFile? Image { get; set; }
    public string? FullName { get; set; }
}
