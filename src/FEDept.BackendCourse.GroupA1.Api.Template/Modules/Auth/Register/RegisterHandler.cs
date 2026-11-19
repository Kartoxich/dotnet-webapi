using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public class RegisterHandler(IUserRepository users, ILogger<RegisterHandler> logger)
{
    public async Task<RegisterResult> RegisterAsync(
        string? email,
        string? password,
        CancellationToken ct)
    {
        // TODO: Move email normalization to service
        var normalizedEmail = EmailNormalizer.Normalize(email);

        // request validator should validate request body
        // #1 - Keep it (application validation)
        // #2 - Validation in service -> throw exception -> map exception into map level exception
        // #3 - combination
        if (RegisterValidator.Validate(normalizedEmail, password) is { } errors)
        {
            logger.LogWarning("Register rejected: {Fields}", string.Join(",", errors.Keys));
            return RegisterResult.InvalidRequest(errors);
        }

        if (await users.FindIdByEmailAsync(normalizedEmail, ct) is not null)
        {
            throw new EmailAlreadyTakenException(normalizedEmail);
        }

        // TODO: Move to password service
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        // Move to user service
        var id = await users.CreateAsync(normalizedEmail, passwordHash, ct);
        logger.LogInformation("User registered: {UserId}", id);

        return RegisterResult.Success(id, normalizedEmail, UserStatus.Pending);
    }
}
