using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public enum RegisterOutcome { Success, InvalidRequest }

public sealed record RegisterResult(
    RegisterOutcome Outcome,
    Guid? Id = null,
    string? Email = null,
    UserStatus? Status = null,
    ConfirmationTicket? Ticket = null,
    IDictionary<string, string[]>? Errors = null)
{
    public static RegisterResult Pending(Guid id, string email, ConfirmationTicket ticket)
        => new(RegisterOutcome.Success, id, email, UserStatus.Pending, ticket);

    public static RegisterResult InvalidRequest(IDictionary<string, string[]> errors)
        => new(RegisterOutcome.InvalidRequest, Errors: errors);
}
