using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Dto.Inviation;
using IssueTracker.Application.Common.Exceptions;
using IssueTracker.Application.Common.Interfaces;
using IssueTracker.Application.Common.Services;
using IssueTracker.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Projects.Commands;

public class InviteUserToProjectCommand : IRequest<InvitationDto>
{
	public Guid ProjectId { get; set; }

	public string RecipientEmail { get; set; } = string.Empty;

	public Guid ProjectRoleId { get; set; }
}

public class InviteUserToProjectCommandHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUserService,
	IEmailService emailService) : IRequestHandler<InviteUserToProjectCommand, InvitationDto>
{
	public async Task<InvitationDto> Handle(InviteUserToProjectCommand request, CancellationToken cancellationToken)
	{
		var currentUserId = currentUserService.GetUserId();
		if (currentUserId == Guid.Empty)
		{
			throw new UnauthorizedAccessException("User is not authenticated");
		}

		// Verify project exists and get project details
		var project = await dbContext.Projects
			.Include(p => p.Owner)
			.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

		if (project == null)
		{
			throw new NotFoundException($"Project with ID {request.ProjectId} not found");
		}

		var sender = await dbContext.Users
			.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

		if (sender == null)
		{
			throw new NotFoundException($"User with ID {currentUserId} not found");
		}

		// Validate project role exists
		var projectRole = await dbContext.ProjectRoles
			.FirstOrDefaultAsync(pr => pr.Id == request.ProjectRoleId, cancellationToken);

		if (projectRole == null)
		{
			throw new NotFoundException($"Project role with ID {request.ProjectRoleId} not found");
		}

		var existingInvitation = await dbContext.Invitations
			.FirstOrDefaultAsync(i => i.ProjectId == request.ProjectId
				&& i.RecipientEmail == request.RecipientEmail
				&& i.Status == Domain.Entities.Enum.StatusOfInvitation.Pending,
				cancellationToken);

		if (existingInvitation != null)
		{
			throw new InvalidOperationException($"An invitation has already been sent to {request.RecipientEmail} for this project");
		}

		var existingMember = await dbContext.UserProjects
			.AnyAsync(up => up.ProjectId == request.ProjectId
				&& up.User.Email == request.RecipientEmail,
				cancellationToken);

		if (existingMember)
		{
			throw new InvalidOperationException($"User with email {request.RecipientEmail} is already a member of this project");
		}

		var invitation = InvitationJoiningProject.Create(
			request.ProjectId,
			currentUserId,
			request.RecipientEmail,
			request.ProjectRoleId);

		dbContext.Invitations.Add(invitation);
		await dbContext.SaveChangesAsync(cancellationToken);

		await emailService.SendProjectInvitationEmailAsync(
			request.RecipientEmail,
			sender.Email,
			project.Name,
			projectRole.Description);

		return new InvitationDto
		{
			Id = invitation.Id,
			ProjectId = invitation.ProjectId,
			SenderId = invitation.SenderId,
			SenderEmail = sender.Email,
			SenderName = sender.FullName ?? sender.Username,
			RecipientEmail = invitation.RecipientEmail,
			StatusOfInvitation = invitation.Status,
			ProjectRoleId = invitation.ProjectRoleId,
			ProjectRoleName = projectRole.Description
		};
	}
}

