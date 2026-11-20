namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public interface IEmailSender
{
    Task SendConfirmationAsync(string to, string code, CancellationToken ct);
}
