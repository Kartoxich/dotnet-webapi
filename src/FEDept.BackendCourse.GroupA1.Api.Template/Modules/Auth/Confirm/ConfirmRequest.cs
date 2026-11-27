namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public sealed record ConfirmRequest(Guid? AttemptId, string? Code);
