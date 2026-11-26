using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public sealed record ConfirmationMessage(
    string Email,
    ChallengeMethod Method,
    string Secret,
    Guid AttemptId,
    DateTimeOffset ExpiresAt,
    TimeSpan Ttl)
{
    public ConfirmationTicket Ticket => new(Method, AttemptId, ExpiresAt, Ttl);
}

public sealed record ConfirmationTicket(
    ChallengeMethod Method,
    Guid AttemptId,
    DateTimeOffset ExpiresAt,
    TimeSpan Ttl);
