using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Domain;
using Microsoft.Extensions.Options;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public sealed class RegisterHandler(
    IUserRepository users,
    IChallengeRepository challenges,
    IPasswordHasher hasher,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    IOptions<CodeOptions> codeOptions,
    IOptions<ResendOptions> resendOptions,
    IOptions<RegistrationOptions> registrationOptions,
    ILogger<RegisterHandler> logger)
{
    private readonly CodeOptions _codeOptions = codeOptions.Value;
    private readonly ResendOptions _resendOptions = resendOptions.Value;
    private readonly RegistrationOptions _registrationOptions = registrationOptions.Value;

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

        UserStatus status = _registrationOptions.RequireConfirmation
            ? UserStatus.Pending
            : UserStatus.Active;

        Guid userId = await users.CreateAsync(normalized, hasher.Hash(password!), status, ct);

        if (status == UserStatus.Active)
        {
            await unitOfWork.SaveChangesAsync(ct);
            logger.LogInformation("User registered active: {UserId}", userId);
            return RegisterResult.Created(userId, normalized);
        }

        var (code, hash) = ConfirmationCode.New(_codeOptions.KeyBytes);
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(_codeOptions.TtlMinutes);
        var ttl = TimeSpan.FromMinutes(_codeOptions.TtlMinutes);
        var resendAfter = TimeSpan.FromSeconds(_resendOptions.CooldownSeconds);

        await challenges.IssueAsync(userId, hash, expiresAt, TimeSpan.Zero, ct);

        var message = new ConfirmationMessage(
            normalized, ChallengeMethod.Otp, code, userId, expiresAt, ttl, resendAfter);

        try
        {
            await emailSender.SendConfirmationAsync(message, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Confirmation mail failed for {UserId}", userId);
            return RegisterResult.MailFailed(userId, normalized);
        }

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("User registered pending: {UserId}", userId);
        return RegisterResult.Pending(userId, normalized, message.Ticket);
    }
}
