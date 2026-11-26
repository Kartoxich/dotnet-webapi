using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Domain;
using Microsoft.Extensions.Options;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Resend;

public sealed class ResendHandler(
    IUserRepository users,
    IChallengeRepository challenges,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    IOptions<CodeOptions> codeOptions,
    IOptions<ResendOptions> resendOptions,
    ILogger<ResendHandler> logger)
{
    private readonly CodeOptions _codeOptions = codeOptions.Value;
    private readonly ResendOptions _resendOptions = resendOptions.Value;

    public async Task<ResendResult> ResendAsync(string? email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ResendResult.InvalidRequest(new Dictionary<string, string[]>
            {
                ["email"] = ["A value is required."],
            });
        }

        var normalized = EmailNormalizer.Normalize(email);

        Guid? userId = await users.FindPendingIdByEmailAsync(normalized, ct);
        if (userId is null)
        {
            logger.LogInformation("Resend: no pending account for the given email");
            return ResendResult.Accepted;
        }

        var (code, hash) = ConfirmationCode.New(_codeOptions.KeyBytes);
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(_codeOptions.TtlMinutes);
        var ttl = TimeSpan.FromMinutes(_codeOptions.TtlMinutes);

        CodeIssue issued = await challenges.IssueAsync(
            userId.Value, hash, expiresAt,
            TimeSpan.FromSeconds(_resendOptions.CooldownSeconds), ct);

        if (issued == CodeIssue.Throttled)
        {
            logger.LogInformation("Resend: throttled for {UserId}", userId);
            return ResendResult.Accepted;
        }

        var message = new ConfirmationMessage(
            normalized, ChallengeMethod.Otp, code, userId.Value, expiresAt, ttl);

        try
        {
            await emailSender.SendConfirmationAsync(message, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Resend mail failed for {UserId}", userId);
            return ResendResult.MailFailed;
        }

        await unitOfWork.SaveChangesAsync(ct);

        return ResendResult.Accepted;
    }
}
