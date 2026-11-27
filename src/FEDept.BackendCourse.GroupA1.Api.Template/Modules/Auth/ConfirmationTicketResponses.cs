using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;

public static class ConfirmationTicketResponses
{
    public static ConfirmationTicketResponse From(ConfirmationTicket ticket) =>
        new(
            ConfirmationRequired: true,
            Method: ticket.Method.ToColumn(),
            AttemptId: ticket.AttemptId,
            ExpiresInSeconds: (int)ticket.Ttl.TotalSeconds,
            ResendAfterSeconds: (int)ticket.ResendAfter.TotalSeconds);

    public static ConfirmationTicketResponse Neutral() =>
        new(
            ConfirmationRequired: false,
            Method: string.Empty,
            AttemptId: Guid.Empty,
            ExpiresInSeconds: 0,
            ResendAfterSeconds: 0);
}
