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
        var email = EmailNormalizer.Normalize(request.Email);

        var error = RegisterValidator.Validate(email, request.Password);
        if (error is not null)
        {
            logger.LogWarning("register rejected: {Reason}", error);
            return Results.BadRequest(new { error });
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        try
        {
            var id = await users.CreateAsync(
                email, passwordHash, UserStatus.Active, cancellationToken
            );
            logger.LogInformation("User registered: {UserId}", id);

            return Results.Created((Uri?)null, new { id, email, status = UserStatus.Active });
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            logger.LogWarning("register conflict: {Email}", email);
            return Results.Conflict(new { error = "Email is already registered"});
        }
    }
}
