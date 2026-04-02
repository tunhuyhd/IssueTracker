using IssueTracker.Application.Common.Dto.Inviation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Application.Projects.Commands;

public class InviteUserToProjectCommand : IRequest<InvitationDto>
{
	public Guid ProjectId { get; set; }

	public string RecipientEmail { get; set; } = string.Empty;
}
public class InviteUserToProjectCommandHandler : IRequestHandler<InviteUserToProjectCommand, InvitationDto>
{
	public async Task<InvitationDto> Handle(InviteUserToProjectCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
