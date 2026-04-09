using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Exceptions;
using IssueTracker.Application.Common.Services;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Application.Projects.Commands;

public class AcceptInvitationCommand : IRequest<bool>
{
    public Guid InvitationId { get; set; }
}

public class AcceptInvitationCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<AcceptInvitationCommand, bool>
{

    public async Task<bool> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.GetUserId();
        var currentUserEmail = currentUserService.GetUserEmail();

        if (currentUserId == Guid.Empty || string.IsNullOrEmpty(currentUserEmail))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var invitation = await dbContext.Invitations
            .Include(i => i.Project)
            .Include(i => i.ProjectRole)
            .FirstOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken);

        if (invitation == null)
        {
            throw new NotFoundException($"Invitation with ID {request.InvitationId} not found");
        }

        if (invitation.RecipientEmail != currentUserEmail)
        {
            throw new UnauthorizedAccessException("This invitation is not for your email address");
        }

        if (invitation.Status != StatusOfInvitation.Pending)
        {
            throw new InvalidOperationException($"This invitation has already been {invitation.Status.ToString().ToLower()}");
        }

        var existingMembership = await dbContext.UserProjects
            .AnyAsync(up => up.UserId == currentUserId
                && up.ProjectId == invitation.ProjectId,
                cancellationToken);

        if (existingMembership)
        {
            throw new InvalidOperationException("You are already a member of this project");
        }

        // Use the project role from invitation
        var userProject = new UserProject
        {
            UserId = currentUserId,
            ProjectId = invitation.ProjectId,
            ProjectRoleId = invitation.ProjectRole.Id
        };

        dbContext.UserProjects.Add(userProject);

        invitation.Status = StatusOfInvitation.Accepted;

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

