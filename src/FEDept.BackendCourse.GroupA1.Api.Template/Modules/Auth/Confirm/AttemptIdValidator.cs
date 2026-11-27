namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public static class AttemptIdValidator
{
    public static string? Validate(Guid? attemptId) =>
        attemptId is null || attemptId == Guid.Empty ? "Attempt id is required" : null;
}
