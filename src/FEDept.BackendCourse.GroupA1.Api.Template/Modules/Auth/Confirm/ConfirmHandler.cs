using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
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

    public async Task<ConfirmResult> ConfirmAsync(Guid? attemptId, string? code, CancellationToken ct)
    {
        if (ConfirmValidator.Validate(attemptId, code) is { } errors)
        {
            logger.LogWarning("Confirm rejected: {Fields}", string.Join(",", errors.Keys));
            return ConfirmResult.InvalidRequest(errors);
        }

        AttemptInfo? attempt = await challenges.FindLiveAsync(attemptId!.Value, ct);
        if (attempt is null)
        {
            logger.LogWarning("Confirm: no live attempt for the given id");
            return ConfirmResult.InvalidCode;
        }

        if (attempt.Attempts >= _codeOptions.MaxAttempts)
        {
            logger.LogWarning("Confirm: too many attempts");
            return ConfirmResult.AttemptsExhausted;
        }

        if (!CodeProtector.Matches(code!, _codeOptions.KeyBytes, attempt.SecretHash))
        {
            if (!await challenges.RegisterWrongAttemptAsync(
                    attempt.Id, _codeOptions.MaxAttempts, ct))
            {
                logger.LogWarning("Confirm: attempt charge lost");
                return ConfirmResult.InvalidCode;
            }

            if (attempt.Attempts + 1 >= _codeOptions.MaxAttempts)
            {
                logger.LogWarning("Confirm: attempt limit reached, challenge consumed");
                return ConfirmResult.AttemptsExhausted;
            }

            logger.LogWarning("Confirm: code mismatch");
            return ConfirmResult.InvalidCode;
        }

        if (!await challenges.TryMarkConsumedAsync(attempt.Id, ct))
        {
            logger.LogWarning("Confirm: consume lost");
            return ConfirmResult.InvalidCode;
        }

        if (!await users.TryMarkActiveAsync(attempt.UserId, ct))
        {
            logger.LogWarning("Confirm: activation lost");
            return ConfirmResult.InvalidCode;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return ConfirmResult.Success;
    }
}
