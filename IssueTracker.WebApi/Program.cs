using IssueTracker.Application;
using IssueTracker.Infrastructure;
using IssueTracker.Infrastructure.Auth;
using IssueTracker.Infrastructure.Persistence.Context;
using IssueTracker.Infrastructure.Persistence.Seeder;
using IssueTracker.WebApi.Configuration;
using IssueTracker.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins(
				"http://localhost:3000",
				"http://localhost:5173",
				"https://localhost:5173") // Support cả HTTP và HTTPS từ frontend
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwtAuth();

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Đọc JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Đảm bảo Program.cs không crash do null jwtSettings
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
{
	throw new InvalidOperationException("JwtSettings is not properly configured.");
}

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtSettings.Issuer,
		ValidAudience = jwtSettings.Audience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
		ClockSkew = TimeSpan.Zero
	};
});

builder.Services.AddAuthorization();

// Add Application and Infrastructure layers
builder.Services
	.AddApplication()
	.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "IssueTracker API v1");
		c.DisplayRequestDuration();
		c.EnableDeepLinking();
		c.EnableValidator();
		c.EnableTryItOutByDefault();
		c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
	});
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

// Global Exception Handler (phải đặt sau CORS)
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed data in development
if (app.Environment.IsDevelopment())
{
	using (var scope = app.Services.CreateScope())
	{
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await DataSeeder.SeedAsync(context);
	}
}

app.Run();
