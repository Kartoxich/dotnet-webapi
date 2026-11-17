using System.Text;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public class ConfirmHandler
{
    private static int Require(IConfiguration config, string key) =>
        config.GetValue<int?>(key) ?? throw new InvalidOperationException($"Missing {key}");

    private static string RequireString(IConfiguration config, string key) =>
        config[key] is { Length: > 0 } v
            ? v
            : throw new InvalidOperationException($"Missing {key}");

    public static async Task<IResult> HandleAsync(
        ConfirmRequest request,
        UserRepository users,
        ChallengeRepository challenges,
        IConfiguration config,
        ILogger<ConfirmHandler> logger,
        CancellationToken ct)
    {
        var email = EmailNormalizer.Normalize(request.Email);

        IDictionary<string, string[]>? errors = ConfirmValidator.Validate(email, request.Code);
        if (errors is not null)
        {
            logger.LogWarning("Confirm rejected: {Fields}", string.Join(",", errors.Keys));
            return Results.ValidationProblem(errors);
        }

        Guid? userId = await users.FindIdByEmailAsync(email, ct);
        if (userId is null)
        {
            logger.LogWarning("Confirm: no user for given email");
            return ConfirmError.InvalidCode();
        }

        var maxAttempts = Require(config, "Confirmation:MaxAttempts");
        var attempt = await challenges.TakeAttemptAsync(userId.Value, ct);
        if (attempt is null)
        {
            logger.LogWarning("Confirm: attempts error");
            return ConfirmError.InvalidCode();
        }

        if (attempt.Value.Attempts > maxAttempts)
        {
            logger.LogWarning("Confirm: too many attempts");
            return ConfirmError.AttemptsExhausted();
        }

        var codeKey = RequireString(config, "Registration:CodeKey");
        var key = Encoding.UTF8.GetBytes(codeKey);

        if (request.Code is null || !CodeProtector.Matches(request.Code, key, attempt.Value.SecretHash))
        {
            logger.LogWarning("Confirm: invalid or missing code key");
            return ConfirmError.InvalidCode();
        }

        if (
            !await challenges.TryMarkConsumedAsync(attempt.Value.Id, ct)
        )
        {
            logger.LogWarning("Confirm: failed to mark consumed");
            return ConfirmError.InvalidCode();
        }

        if (!await users.TryMarkActiveAsync(userId.Value, ct))
        {
            logger.LogWarning("Confirm: failed to mark active");
            return ConfirmError.InvalidCode();
        }

        await challenges.CommitAsync(ct);

        return Results.Ok(new { status = "active" });
    }
}
