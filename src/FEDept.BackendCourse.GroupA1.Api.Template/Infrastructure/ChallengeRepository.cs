using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Persistence;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

public class ChallengeRepository(AppDbContext db) : IChallengeRepository
{
        public async Task<CodeIssue> IssueAsync(
        Guid userId, byte[] secretHash, DateTimeOffset expiresAt,
        TimeSpan cooldown, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var now = DateTimeOffset.UtcNow;

        // Supersede any live row — expired ones included, because they still
        // occupy the partial unique index slot (consumed_at IS NULL).
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
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg
            && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            await tx.RollbackAsync(ct);
            return CodeIssue.Throttled;
        }

        await tx.CommitAsync(ct);
        return CodeIssue.Issued;
    }

    public async Task<AttemptInfo?> TakeAttemptAsync(
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
        return new AttemptInfo(c.Id, c.SecretHash, c.Attempts);
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
}
