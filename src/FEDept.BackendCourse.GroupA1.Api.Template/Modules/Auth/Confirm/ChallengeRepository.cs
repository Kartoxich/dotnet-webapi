using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public class ChallengeRepository(AppDbContext db)
{
    public async Task<Guid> CreateAsync(
        Guid userId,
        byte[] secretHash,
        ChallengeMethod method,
        DateTimeOffset expiresAt,
        CancellationToken ct
    )
    {
        var challenge = new RegistrationChallenge
        {
            UserId = userId,
            SecretHash = secretHash,
            Method = method,
            ExpiresAt = expiresAt,
            Attempts = 0,
            LastSentAt = DateTimeOffset.UtcNow
        };

        db.RegistrationChallenges.Add(challenge);

        await db.SaveChangesAsync(ct);

        return challenge.Id;
    }

    public async Task<(Guid Id, byte[] SecretHash, int Attempts)?> TakeAttemptAsync(
        Guid userId, CancellationToken ct)
    {
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(ct);

        DateTimeOffset now = DateTimeOffset.UtcNow;

        var rows = await db.RegistrationChallenges
            .Where(c => c.UserId == userId && c.ConsumedAt == null && c.ExpiresAt > now)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(c => c.Attempts, c => c.Attempts + 1), ct);

        if (rows == 0)
        {
            await tx.RollbackAsync(ct); return null;
        }

        var c = await db.RegistrationChallenges.AsNoTracking()
            .Where(x => x.UserId == userId && x.ConsumedAt == null && x.ExpiresAt > now)
            .Select(x => new { x.Id, x.SecretHash, x.Attempts })
            .SingleAsync(ct);

        await tx.CommitAsync(ct);
        return (c.Id, c.SecretHash, c.Attempts);
    }

    public async Task<bool> TryMarkConsumedAsync(Guid challengeId, CancellationToken ct)
    {
        var registrationChallenge = await db.RegistrationChallenges
            .FirstOrDefaultAsync(challenge => challenge.Id == challengeId && challenge.ConsumedAt == null, ct);

        if (registrationChallenge == null)
        {
            return false;
        }

        registrationChallenge.ConsumedAt = DateTimeOffset.UtcNow;
        return true;
    }

    public async Task<Guid?> ResendAsync(
        Guid userId, byte[] secretHash,
        DateTimeOffset expiresAt,
        TimeSpan minInterval,
        CancellationToken ct)
    {
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(ct);
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        DateTimeOffset floor = utcNow - minInterval;

        var rows = await db.RegistrationChallenges
            .Where(rc => rc.UserId == userId && rc.ConsumedAt == null && rc.LastSentAt < floor)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rc => rc.LastSentAt, utcNow)
                .SetProperty(rc => rc.SecretHash, secretHash)
                .SetProperty(rc => rc.ExpiresAt, expiresAt)
                .SetProperty(rc => rc.Attempts, 0), ct);

        if (rows == 0)
        {
            await tx.RollbackAsync(ct);
            return null;
        }

        var id = await db.RegistrationChallenges.AsNoTracking()
            .Where(rc => rc.UserId == userId && rc.ConsumedAt == null && rc.ExpiresAt > DateTimeOffset.UtcNow)
            .Select(rc => rc.Id)
            .SingleAsync(ct);

        await tx.CommitAsync(ct);

        return id;
    }

    public Task CommitAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
