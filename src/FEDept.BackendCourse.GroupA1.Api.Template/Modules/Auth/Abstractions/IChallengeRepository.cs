using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public interface IChallengeRepository
{
    Task<Guid> CreateAsync(Guid userId, byte[] secretHash, ChallengeMethod method,
        DateTimeOffset expiresAt, CancellationToken ct);
    Task<AttemptInfo?> TakeAttemptAsync(Guid userId, CancellationToken ct);
    Task<bool> TryMarkConsumedAsync(Guid challengeId, CancellationToken ct);
    Task<Guid?> ResendAsync(Guid userId, byte[] secretHash, DateTimeOffset expiresAt,
        TimeSpan minInterval, CancellationToken ct);
}
