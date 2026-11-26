using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Persistence;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Repositories;

public sealed class ChallengeRepository(AppDbContext db) : IChallengeRepository
{
    public async Task<CodeIssue> IssueAsync(
        Guid userId, byte[] secretHash, DateTimeOffset expiresAt,
        TimeSpan cooldown, CancellationToken ct)
    {
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(ct);
        var now = DateTimeOffset.UtcNow;

        var rows = await db.RegistrationChallenges
            .Where(c => c.UserId == userId && c.ConsumedAt == null && c.LastSentAt <= now - cooldown)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.LastSentAt, now)
                .SetProperty(c => c.SecretHash, secretHash)
                .SetProperty(c => c.ExpiresAt, expiresAt)
                .SetProperty(c => c.Attempts, 0), ct);

        if (rows > 0)
        {
            await tx.CommitAsync(ct);
            return CodeIssue.Issued;
        }

        var live = await db.RegistrationChallenges
            .AnyAsync(c => c.UserId == userId && c.ConsumedAt == null, ct);

        if (live)
        {
            await tx.RollbackAsync(ct);
            return CodeIssue.Throttled;
        }

        db.RegistrationChallenges.Add(new RegistrationChallenge
        {
            UserId = userId,
            SecretHash = secretHash,
            Method = ChallengeMethod.Otp,
            ExpiresAt = expiresAt,
            Attempts = 0,
            LastSentAt = now,
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg
                                           && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            await tx.RollbackAsync(ct);
            return CodeIssue.Throttled;
        }

        await tx.CommitAsync(ct);
        return CodeIssue.Issued;
    }

    public async Task<AttemptInfo?> FindLiveAsync(Guid userId, CancellationToken ct)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return await db.RegistrationChallenges.AsNoTracking()
            .Where(c => c.UserId == userId && c.ConsumedAt == null && c.ExpiresAt > now)
            .Select(c => new AttemptInfo(c.Id, c.SecretHash, c.Attempts))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> RegisterWrongAttemptAsync(
        Guid userId, int maxAttempts, CancellationToken ct)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var rows = await db.RegistrationChallenges
            .Where(c => c.UserId == userId && c.ConsumedAt == null && c.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Attempts, c => c.Attempts + 1)
                .SetProperty(c => c.ConsumedAt,
                    c => c.Attempts + 1 >= maxAttempts ? now : c.ConsumedAt), ct);

        return rows > 0;
    }

    public async Task<bool> TryMarkConsumedAsync(Guid challengeId, CancellationToken ct)
    {
        RegistrationChallenge? registrationChallenge = await db.RegistrationChallenges
            .FirstOrDefaultAsync(
                challenge => challenge.Id == challengeId && challenge.ConsumedAt == null, ct);

        if (registrationChallenge is null)
        {
            return false;
        }

        registrationChallenge.ConsumedAt = DateTimeOffset.UtcNow;
        return true;
    }
}
