using Microsoft.OpenApi.Models;
using System.Reflection;

namespace IssueTracker.WebApi.Configuration;

public static class SwaggerConfiguration
{
	public static IServiceCollection AddSwaggerWithJwtAuth(this IServiceCollection services)
	{
		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "IssueTracker API",
				Version = "v1",
				Description = "Issue Tracking System API with JWT Authentication - .NET 10",
				Contact = new OpenApiContact
				{
					Name = "IssueTracker Support",
					Email = "support@issuetracker.com"
				},
				License = new OpenApiLicense
				{
					Name = "MIT License",
					Url = new Uri("https://opensource.org/licenses/MIT")
				}
			});

			// Include XML comments if available
			var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
			if (File.Exists(xmlPath))
			{
				c.IncludeXmlComments(xmlPath);
			}

			// Define the Bearer token security scheme
			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Name = "Authorization",
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
			});

			// Apply the security scheme globally
			c.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					Array.Empty<string>()
				}
			});

			// Improve schema generation
			c.CustomSchemaIds(type => type.FullName);
		});

		return services;
	}
}
