using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Domain;
using Microsoft.Extensions.Options;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public sealed class ConfirmHandler(
    IUserRepository users,
    IChallengeRepository challenges,
    IUnitOfWork unitOfWork,
    IOptions<CodeOptions> codeOptions,
    ILogger<ConfirmHandler> logger)
{
    private readonly CodeOptions _codeOptions = codeOptions.Value;

    public async Task<ConfirmResult> ConfirmAsync(string? email, string? code, CancellationToken ct)
    {
        var normalized = EmailNormalizer.Normalize(email);

        if (ConfirmValidator.Validate(normalized, code) is { } errors)
        {
            logger.LogWarning("Confirm rejected: {Fields}", string.Join(",", errors.Keys));
            return ConfirmResult.InvalidRequest(errors);
        }

        Guid? userId = await users.FindIdByEmailAsync(normalized, ct);
        if (userId is null)
        {
            logger.LogWarning("Confirm: no user for given email");
            return ConfirmResult.InvalidCode;
        }

        AttemptInfo? attemptInfo = await challenges.TakeAttemptAsync(userId.Value, ct);
        if (attemptInfo is null)
        {
            logger.LogWarning("Confirm: no live challenge");
            return ConfirmResult.InvalidCode;
        }

        if (attemptInfo.Attempts > _codeOptions.MaxAttempts)
        {
            logger.LogWarning("Confirm: too many attempts");
            return ConfirmResult.AttemptsExhausted;
        }

        if (code is null || !CodeProtector.Matches(code, _codeOptions.KeyBytes, attemptInfo.SecretHash))
        {
            logger.LogWarning("Confirm: code mismatch");
            return ConfirmResult.InvalidCode;
        }

        if (!await challenges.TryMarkConsumedAsync(attemptInfo.Id, ct))
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
