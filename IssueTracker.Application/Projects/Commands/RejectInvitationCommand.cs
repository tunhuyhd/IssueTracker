using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Exceptions;
using IssueTracker.Application.Common.Services;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Application.Projects.Commands;

public class RejectInvitationCommand : IRequest<bool>
{
    public Guid InvitationId { get; set; }
}

public class RejectInvitationCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<RejectInvitationCommand, bool>
{
    public async Task<bool> Handle(RejectInvitationCommand request, CancellationToken cancellationToken)
    {
        var currentUserEmail = currentUserService.GetUserEmail();

        if (string.IsNullOrEmpty(currentUserEmail))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var invitation = await dbContext.Invitations
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

        invitation.Status = StatusOfInvitation.Rejected;

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
