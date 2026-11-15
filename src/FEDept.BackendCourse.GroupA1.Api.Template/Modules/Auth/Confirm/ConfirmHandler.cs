using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public class ConfirmHandler
{
    public static async Task<IResult> HandleAsync(
        ConfirmRequest request,
        ChallengeRepository challenges,
        ILogger<ConfirmHandler> logger,
        CancellationToken cancellationToken)
    {
        var email = EmailNormalizer.Normalize(request.Email);

        IDictionary<string, string[]>? errors = ConfirmValidator.Validate(email, request.Code);
        if (errors is not null)
        {
            return Results.ValidationProblem(errors);
        }

        return Results.NotFound();
    }
}
