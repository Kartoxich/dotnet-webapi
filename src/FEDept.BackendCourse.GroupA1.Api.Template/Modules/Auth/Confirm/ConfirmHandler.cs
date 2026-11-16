using System.Text;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
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
        CancellationToken cancellationToken)
    {
        var email = EmailNormalizer.Normalize(request.Email);

        IDictionary<string, string[]>? errors = ConfirmValidator.Validate(email, request.Code);
        if (errors is not null)
        {
            logger.LogWarning("Confirm rejected: {Fields}", string.Join(",", errors));
            return Results.ValidationProblem(errors);
        }

        Guid? userId = await users.FindIdByEmailAsync(email, cancellationToken);
        if (userId is null)
        {
            logger.LogWarning("Confirm: no user for given email");
            return ConfirmError.InvalidCode();
        }

        var maxAttempts = Require(config, "Confirmation:MaxAttempts");
        var attempt = await challenges.TakeAttemptAsync(userId.Value, cancellationToken);
        if (attempt is null)
            return ConfirmError.InvalidCode();

        if (attempt.Value.Attempts > maxAttempts)
            return ConfirmError.AttemptsExhausted(TimeSpan.FromMinutes(10));
        var codeKey = RequireString(config, "Registration:CodeKey");
        var key = Encoding.UTF8.GetBytes(codeKey);

        if (request.Code != null && !CodeProtector.Matches(request.Code, key, attempt.Value.SecretHash))
            return ConfirmError.InvalidCode();

        if (!await challenges.ConsumeAsync(attempt.Value.ChallengeId, cancellationToken))
            return ConfirmError.InvalidCode();

        await users.ActivateAsync(userId.Value, cancellationToken);
        return Results.Ok(new { status = "active" });
    }
}
