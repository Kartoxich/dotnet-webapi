namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Resend;

public enum ResendOutcome { Accepted, InvalidRequest }

public sealed record ResendResult(
    ResendOutcome Outcome,
    IDictionary<string, string[]>? Errors = null)
{
    public static readonly ResendResult Accepted = new(ResendOutcome.Accepted);
    public static ResendResult InvalidRequest(IDictionary<string, string[]> e)
        => new(ResendOutcome.InvalidRequest, e);
}
