using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Domain;
using Microsoft.Extensions.Options;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public sealed class RegisterHandler(
    IUserRepository users,
    IChallengeRepository challenges,
    IPasswordHasher hasher,
    IEmailSender emailSender,
    IOptions<CodeOptions> codeOptions,
    ILogger<RegisterHandler> logger)
{
    private readonly CodeOptions _codeOptions = codeOptions.Value;

    public async Task<RegisterResult> RegisterAsync(string? email, string? password, CancellationToken ct)
    {
        var normalized = EmailNormalizer.Normalize(email);

        if (RegisterValidator.Validate(normalized, password) is { } errors)
        {
            logger.LogWarning("Register rejected: {Fields}", string.Join(",", errors.Keys));
            return RegisterResult.InvalidRequest(errors);
        }

        if (await users.FindIdByEmailAsync(normalized, ct) is not null)
        {
            throw new EmailAlreadyTakenException(normalized);
        }

        Guid userId = await users.CreateAsync(normalized, hasher.Hash(password!), ct);

        var (code, hash) = ConfirmationCode.New(_codeOptions.KeyBytes);
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(_codeOptions.TtlMinutes);

        await challenges.IssueAsync(userId, hash, expiresAt, TimeSpan.Zero, ct);

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
