using System.Text;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public class ConfirmHandler(
    IUserRepository users,
    IChallengeRepository challenges,
    IUnitOfWork unitOfWork,
    IConfiguration config,
    ILogger<ConfirmHandler> logger)
{
    private int RequireInt(string key) =>
        config.GetValue<int?>(key) ?? throw new InvalidOperationException($"Missing {key}");

    private string RequireString(string key) =>
        config[key] is { Length: > 0 } v
            ? v
            : throw new InvalidOperationException($"Missing {key}");

    public async Task<ConfirmResult> ConfirmAsync(string? email, string? code, CancellationToken ct)
    {
        var normalized = EmailNormalizer.Normalize(email);

        if (ConfirmValidator.Validate(normalized, code) is { } errors)
        {
            logger.LogWarning("Confirm rejected: {Fields}", string.Join(",", errors.Keys));
            return ConfirmResult.InvalidRequest(errors);
        }

        var userId = await users.FindIdByEmailAsync(normalized, ct);
        if (userId is null)
        {
            logger.LogWarning("Confirm: no user for given email");
            return ConfirmResult.InvalidCode;
        }

        var attempt = await challenges.TakeAttemptAsync(userId.Value, ct);
        if (attempt is null)
        {
            logger.LogWarning("Confirm: no live challenge");
            return ConfirmResult.InvalidCode;
        }

        if (attempt.Attempts > RequireInt("Confirmation:MaxAttempts"))
        {
            logger.LogWarning("Confirm: too many attempts");
            return ConfirmResult.AttemptsExhausted;
        }

        var key = Encoding.UTF8.GetBytes(RequireString("Registration:CodeKey"));
        if (code is null || !CodeProtector.Matches(code, key, attempt.SecretHash))
        {
            logger.LogWarning("Confirm: code mismatch");
            return ConfirmResult.InvalidCode;
        }

        if (!await challenges.TryMarkConsumedAsync(attempt.Id, ct))
        {
            logger.LogWarning("Confirm: consume lost");
            return ConfirmResult.InvalidCode;
        }

        if (!await users.TryMarkActiveAsync(userId.Value, ct))
        {
            logger.LogWarning("Confirm: activation lost");
            return ConfirmResult.InvalidCode;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return ConfirmResult.Success;
    }
}
