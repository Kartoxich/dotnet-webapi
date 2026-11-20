using System.Security.Cryptography;
using System.Text;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public class RegisterHandler(
    IUserRepository users,
    IChallengeRepository challenges,
    IPasswordHasher hasher,
    IEmailSender emailSender,
    IConfiguration config,
    ILogger<RegisterHandler> logger)
{
    private int RequireInt(string key) =>
        config.GetValue<int?>(key) ?? throw new InvalidOperationException($"Missing {key}");

    private string RequireString(string key) =>
        config[key] is { Length: > 0 } v
            ? v
            : throw new InvalidOperationException($"Missing {key}");

    public async Task<RegisterResult> RegisterAsync(string? email, string? password, CancellationToken ct)
    {
        var normalized = EmailNormalizer.Normalize(email);

        if (RegisterValidator.Validate(normalized, password) is { } errors)
        {
            logger.LogWarning("Register rejected: {Fields}", string.Join(",", errors.Keys));
            return RegisterResult.InvalidRequest(errors);
        }

        if (await users.FindIdByEmailAsync(normalized, ct) is not null)
            throw new EmailAlreadyTakenException(normalized);

        var userId = await users.CreateAsync(normalized, hasher.Hash(password!), ct);

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var key = Encoding.UTF8.GetBytes(RequireString("Registration:CodeKey"));
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(RequireInt("Confirmation:CodeTtlMinutes"));

        await challenges.CreateAsync(userId, CodeProtector.Protect(code, key),
            ChallengeMethod.Otp, expiresAt, ct);

        try
        {
            await emailSender.SendConfirmationAsync(normalized, code, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Confirmation mail failed for {UserId}", userId);
        }

        logger.LogInformation("User registered: {UserId}", userId);
        return RegisterResult.Success(userId, normalized, UserStatus.Pending);
    }
}
