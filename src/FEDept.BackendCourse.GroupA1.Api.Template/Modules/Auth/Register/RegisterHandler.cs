using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;
using Npgsql;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public class RegisterHandler
{
    public static async Task<IResult> HandleAsync(
        RegisterRequest request,
        UserRepository users,
        ILogger<RegisterHandler> logger,
        CancellationToken cancellationToken)
    {
        // TODO: Move email normalization to service
        var email = EmailNormalizer.Normalize(request.Email);

        // request validator should validate request body
        // #1 - Keep it (application validation)
        // #2 - Validation in service -> throw exception -> map exception into map level exception
        // #3 - combination
        var errors = RegisterValidator.Validate(email, request.Password);
        if (errors is not null)
        {
            logger.LogWarning("Register rejected: {Fields}", string.Join(",", errors.Keys));
            return Results.ValidationProblem(errors);
        }

        // TODO: Move to password service
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        try
        {
            // TODO: move to user service
            var id = await users.CreateAsync(
                email, passwordHash, cancellationToken
            );
            logger.LogInformation("User registered: {UserId}", id);

            return Results.Created((Uri?)null, new { id, email, status = UserStatus.Active });
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation) // TODO: move to service
        {
            logger.LogWarning("register conflict: {Email}", email);
            return Results.Problem(
                title: "Email is already registered",
                type: RegisterErrorTypes.EmailTaken,
                statusCode: StatusCodes.Status409Conflict
            );
        }
    }
}
