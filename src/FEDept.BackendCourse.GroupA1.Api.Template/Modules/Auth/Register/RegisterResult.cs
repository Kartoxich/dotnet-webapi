namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public enum RegisterOutcome { Success, InvalidRequest }

public sealed record RegisterResult(
    RegisterOutcome Outcome,
    Guid? Id = null,
    string? Email = null,
    UserStatus? Status = null,
    IDictionary<string, string[]>? Errors = null)
{
    public static RegisterResult Success(Guid id, string email, UserStatus status)
        => new(RegisterOutcome.Success, id, email, status);
    public static RegisterResult InvalidRequest(IDictionary<string, string[]> errors)
        => new(RegisterOutcome.InvalidRequest, Errors: errors);
}
