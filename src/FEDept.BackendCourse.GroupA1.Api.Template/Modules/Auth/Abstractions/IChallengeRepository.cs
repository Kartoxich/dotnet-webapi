using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public interface IChallengeRepository
{
    Task<CodeIssue> IssueAsync(
        Guid userId,
        byte[] secretHash,
        DateTimeOffset expiresAt,
        TimeSpan cooldown,
        CancellationToken ct);

    Task<AttemptInfo?> FindLiveAsync(Guid userId, CancellationToken ct);

    Task<bool> RegisterWrongAttemptAsync(Guid userId, int maxAttempts, CancellationToken ct);

    Task<bool> TryMarkConsumedAsync(Guid challengeId, CancellationToken ct);
}
