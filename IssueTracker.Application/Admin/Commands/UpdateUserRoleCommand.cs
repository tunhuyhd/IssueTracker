using IssueTracker.Application.Common.Authorization;
using IssueTracker.Application.Common.Dto.Users;
using IssueTracker.Domain.Common;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;

namespace IssueTracker.Application.Admin.Commands;

/// <summary>
/// Command to update a user's role (Admin only)
/// </summary>
public class UpdateUserRoleCommand : IRequest<UserDto>
{
	public Guid UserId { get; set; }
	public Guid RoleId { get; set; }
}

public class UpdateUserRoleCommandHandler(
	IRepository<User> userRepository,
	IRepository<Role> roleRepository,
	ICurrentUser currentUser) : IRequestHandler<UpdateUserRoleCommand, UserDto>
{
	public async Task<UserDto> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
	{
		// Verify current user is Admin
		if (currentUser.GetRoleCode() != RoleCode.Admin)
		{
			throw new UnauthorizedAccessException("Only admins can update user roles.");
		}

		// Get the user to update
		var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
		if (user == null)
		{
			throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
		}

		// Verify the new role exists
		var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
		if (role == null)
		{
			throw new InvalidOperationException($"Role with ID {request.RoleId} does not exist.");
		}

		// Prevent admin from changing their own role
		var currentUserId = currentUser.GetUserId();
		if (user.Id == currentUserId)
		{
			throw new InvalidOperationException("You cannot change your own role.");
		}

		// Update role using domain method
		user.UpdateRole(request.RoleId);

		await userRepository.UpdateAsync(user, cancellationToken);
		await userRepository.SaveChangesAsync(cancellationToken);

		return new UserDto
		{
			Id = user.Id,
			Username = user.Username,
			Email = user.Email,
			FullName = user.FullName,
			ImageUrl = user.ImageUrl,
			IsActive = user.IsActive,
			RoleId = user.RoleId,
			RoleCode = role.Code,
			RoleName = role.Description
		};
	}
}
