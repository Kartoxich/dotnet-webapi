using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public class ConfirmHandler
{
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

        var maxAttempts = config.GetValue<int?>("Confirmation:MaxAttempts")
            ?? throw new InvalidOperationException("Missing Confirmation:MaxAttempts");
        var attempts = await challenges.TakeAttemptAsync(
            userId.Value, maxAttempts, cancellationToken);
        if (attempts is null)
        {
            logger.LogWarning("Confirm: no challenges for given user id");
            return ConfirmError.InvalidCode();
        }

        return Results.NotFound();
    }
}
