using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto;
using IssueTracker.Application.Common.Dto.Users;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Admin.Queries;

public class GetListUserQuery : IRequest<PagedResult<UserDto>>
{
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public string? SearchTerm { get; set; }
	public string? RoleCode { get; set; }
}

public class GetListUserQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) 
	: IRequestHandler<GetListUserQuery, PagedResult<UserDto>>
{
	public async Task<PagedResult<UserDto>> Handle(GetListUserQuery request, CancellationToken cancellationToken)
	{
		var currentUserRole = currentUser.GetRoleCode();

		if (currentUserRole != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only Admin can access this resource.");
		}

		// Validate pagination parameters
		if (request.Page < 1)
			request.Page = 1;
		if (request.PageSize < 1)
			request.PageSize = 10;
		if (request.PageSize > 100)
			request.PageSize = 100;

		var query = dbContext.Users.Include(u => u.Role).AsQueryable();

		// Filter by search term (username, email, or full name)
		if (!string.IsNullOrWhiteSpace(request.SearchTerm))
		{
			var searchTerm = request.SearchTerm.ToLower();
			query = query.Where(u =>
				u.Username.ToLower().Contains(searchTerm) ||
				u.Email.ToLower().Contains(searchTerm) ||
				(u.FullName != null && u.FullName.ToLower().Contains(searchTerm))
			);
		}

		// Filter by role code
		if (!string.IsNullOrWhiteSpace(request.RoleCode))
		{
			query = query.Where(u => u.Role.Code == request.RoleCode);
		}

		// Get total count before pagination
		var totalCount = await query.CountAsync(cancellationToken);

		// Apply pagination
		var users = await query
			.OrderBy(u => u.CreatedOn)
			.Skip((request.Page - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToListAsync(cancellationToken);

		var userDtos = users.Select(u => new UserDto
		{
			Id = u.Id,
			Username = u.Username,
			FullName = u.FullName,
			Email = u.Email,
			ImageUrl = u.ImageUrl,
			RoleId = u.RoleId,
			RoleCode = u.Role.Code,
			RoleName = u.Role.Description,
			IsActive = u.IsActive,
			CreatedAt = u.CreatedOn
		}).ToList();

		return new PagedResult<UserDto>
		{
			Items = userDtos,
			TotalCount = totalCount,
			Page = request.Page,
			PageSize = request.PageSize
		};
	}
}