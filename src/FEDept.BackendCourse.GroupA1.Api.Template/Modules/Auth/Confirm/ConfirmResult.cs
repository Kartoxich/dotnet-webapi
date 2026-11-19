namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public enum ConfirmOutcome
{
    Success,
    InvalidRequest,
    InvalidCode,
    AttemptsExhausted,
}

public sealed record ConfirmResult(
    ConfirmOutcome Outcome,
    IDictionary<string, string[]>? Errors = null)
{
    public static readonly ConfirmResult Success = new(ConfirmOutcome.Success);
    public static readonly ConfirmResult InvalidCode = new(ConfirmOutcome.InvalidCode);
    public static readonly ConfirmResult AttemptsExhausted = new(ConfirmOutcome.AttemptsExhausted);
    public static ConfirmResult InvalidRequest(IDictionary<string, string[]> e)
        => new(ConfirmOutcome.InvalidRequest, e);
}
