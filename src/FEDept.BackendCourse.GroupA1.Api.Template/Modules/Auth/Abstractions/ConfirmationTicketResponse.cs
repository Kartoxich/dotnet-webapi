namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public sealed record ConfirmationTicketResponse(
    bool ConfirmationRequired,
    string Method,
    Guid AttemptId,
    int ExpiresInSeconds,
    int ResendAfterSeconds);
