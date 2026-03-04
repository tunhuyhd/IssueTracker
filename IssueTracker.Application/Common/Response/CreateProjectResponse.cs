using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Common.Response;

public record CreateProjectResponse
{
	public Guid Id { get; init; }
	public string Name { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
	public string OwnerId { get; init; } = string.Empty;
	public string OwnerName { get; init; } = string.Empty;
}
