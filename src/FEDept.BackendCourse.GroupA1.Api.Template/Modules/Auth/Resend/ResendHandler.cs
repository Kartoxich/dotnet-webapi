using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Domain;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Resend;

public class ResendHandler(
    IUserRepository users,
    IChallengeRepository challenges,
    IEmailSender emailSender,
    CodeOptions options,
    ILogger<ResendHandler> logger)
{
    public async Task<ResendResult> ResendAsync(string? email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ResendResult.InvalidRequest(new Dictionary<string, string[]>
            {
                ["email"] = ["A value is required."],
            });

        var normalized = EmailNormalizer.Normalize(email);

        var userId = await users.FindPendingIdByEmailAsync(normalized, ct);
        if (userId is null)
        {
            // unknown address OR already active — indistinguishable by construction
            logger.LogInformation("Resend: no pending account for the given email");
            return ResendResult.Accepted;
        }

        var (code, hash) = ConfirmationCode.New(options.CodeKeyBytes);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.CodeTtlMinutes);

        var issued = await challenges.IssueAsync(
            userId.Value, hash, expiresAt,
            TimeSpan.FromSeconds(options.ResendCooldownSeconds), ct);

        if (issued == CodeIssue.Throttled)
        {
            logger.LogInformation("Resend: throttled for {UserId}", userId);
            return ResendResult.Accepted;
        }

        try
        {
            await emailSender.SendConfirmationAsync(normalized, code, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Resend mail failed for {UserId}", userId);
        }

        return ResendResult.Accepted;
    }
}
