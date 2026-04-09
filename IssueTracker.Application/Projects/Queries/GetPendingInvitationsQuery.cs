using IssueTracker.Application.Common;
using IssueTracker.Application.Common.Dto.Inviation;
using IssueTracker.Application.Common.Services;
using IssueTracker.Domain.Entities.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Application.Projects.Queries;

public class GetPendingInvitationsQuery : IRequest<List<InvitationDto>>
{

}

public class GetPendingInvitationsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<GetPendingInvitationsQuery, List<InvitationDto>>
{
    public async Task<List<InvitationDto>> Handle(GetPendingInvitationsQuery request, CancellationToken cancellationToken)
    {
        var currentUserEmail = currentUserService.GetUserEmail();

        if (string.IsNullOrEmpty(currentUserEmail))
        {
            return new List<InvitationDto>();
        }

        var invitations = await dbContext.Invitations
            .AsNoTracking()
            .Include(i => i.Project)
            .Include(i => i.Sender)
            .Include(i => i.ProjectRole)
            .Where(i => i.RecipientEmail == currentUserEmail
                && i.Status == StatusOfInvitation.Pending)
            .OrderByDescending(i => i.CreatedOn)
            .ToListAsync(cancellationToken);

        return invitations.Select(i => new InvitationDto
        {
            Id = i.Id,
            ProjectId = i.ProjectId,
            SenderId = i.SenderId,
            SenderEmail = i.Sender.Email,
            SenderName = i.Sender.FullName ?? i.Sender.Username,
            RecipientEmail = i.RecipientEmail,
            StatusOfInvitation = i.Status,
            ProjectRoleId = i.ProjectRoleId,
            ProjectRoleName = i.ProjectRole.Description
        }).ToList();
    }
}
