namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public interface IEmailSender
{
    Task SendConfirmationAsync(ConfirmationMessage message, CancellationToken ct);
}
