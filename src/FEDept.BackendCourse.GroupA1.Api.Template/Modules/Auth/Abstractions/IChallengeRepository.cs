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

    Task<AttemptInfo?> FindLiveAsync(Guid attemptId, CancellationToken ct);

    Task<bool> RegisterWrongAttemptAsync(Guid attemptId, int maxAttempts, CancellationToken ct);

    Task<bool> TryMarkConsumedAsync(Guid attemptId, CancellationToken ct);
}
