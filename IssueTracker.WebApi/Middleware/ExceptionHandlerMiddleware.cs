using IssueTracker.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace IssueTracker.WebApi.Middleware;

public class ExceptionHandlerMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlerMiddleware> _logger;

	public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			// Log khác nhau tùy loại exception
			LogException(ex);
			await HandleExceptionAsync(context, ex);
		}
	}

	private void LogException(Exception exception)
	{
		// Không log error cho expected exceptions (UnauthorizedException, NotFoundException, ValidationException)
		// Chỉ log warning hoặc information
		switch (exception)
		{
			case UnauthorizedException:
				// Đã log ở LoginCommandHandler rồi, không cần log lại
				break;
			case NotFoundException:
				_logger.LogWarning("Resource not found: {Message}", exception.Message);
				break;
			case ValidationException:
				_logger.LogWarning("Validation failed: {Message}", exception.Message);
				break;
			default:
				// Chỉ log error cho unexpected exceptions
				_logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
				break;
		}
	}

	private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		// Kiểm tra nếu response đã bắt đầu được ghi thì không xử lý nữa
		if (context.Response.HasStarted)
		{
			return;
		}

		// Đảm bảo CORS headers được thêm vào
		context.Response.Headers.Append("Access-Control-Allow-Origin", context.Request.Headers["Origin"].ToString());
		context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");

		context.Response.ContentType = "application/json";

		var (statusCode, message) = exception switch
		{
			UnauthorizedException => (HttpStatusCode.Unauthorized, exception.Message),
			NotFoundException => (HttpStatusCode.NotFound, exception.Message),
			ValidationException validationException => (
				HttpStatusCode.BadRequest,
				validationException.Errors.Any()
					? string.Join("; ", validationException.Errors)
					: validationException.Message
			),
			_ => (HttpStatusCode.InternalServerError, "An internal server error occurred.")
		};

		context.Response.StatusCode = (int)statusCode;

		var response = new
		{
			statusCode = (int)statusCode,
			message = message,
			timestamp = DateTime.UtcNow
		};

		var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});

		await context.Response.WriteAsync(jsonResponse);
		await context.Response.CompleteAsync(); // Đảm bảo response được gửi đi
	}
}
