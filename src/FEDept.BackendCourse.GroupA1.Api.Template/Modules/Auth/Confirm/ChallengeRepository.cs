using Npgsql;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public class ChallengeRepository(string connectionString)
{
    private const string InsertSql = """
        INSERT INTO registration_challenges (user_id, secret_hash, method, expires_at)
        VALUES (@user_id, @secret_hash, @method, @expires_at)
        RETURNING registration_challenge_id
    """;

    private const string TakeAttemptSql = """
        UPDATE registration_challenges
        SET attempts = attempts + 1
        WHERE user_id = @user_id
            AND consumed_at IS NULL
            AND expires_at > now()
            AND attempts < @max_attempts
        RETURNING registration_challenge_id, secret_hash
    """;

    private const string ConsumeSql = """
        UPDATE registration_challenges
        SET consumed_at = now()
        WHERE registration_challenge_id = @challenge_id
            AND consumed_at IS NULL
    """;

    private const string ResendSql = """
        UPDATE registration_challenges
        SET last_sent_at = now(), secret_hash = @secret_hash,
            expires_at = @expires_at, attempts = 0
        WHERE user_id = @user_id
            AND consumed_at IS NULL
            AND last_sent_at < now() - @min_interval
        RETURNING registration_challenge_id
    """;

    public async Task<Guid> CreateAsync(
        Guid userId,
        byte[] secretHash,
        ChallengeMethod method,
        DateTimeOffset expiresAt,
        CancellationToken ct
    )
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(InsertSql, connection);

        cmd.Parameters.AddWithValue("@user_id", userId);
        cmd.Parameters.AddWithValue("@secret_hash", secretHash);
        cmd.Parameters.AddWithValue("@method", method.ToColumn());
        cmd.Parameters.AddWithValue("@expires_at", expiresAt);

        return (Guid)(await cmd.ExecuteScalarAsync(ct) ?? throw new InvalidOperationException());
    }

    public async Task<(Guid ChallengeId, byte[] SecretHash)?> TakeAttemptAsync(
        Guid userId,
        int maxAttempts,
        CancellationToken ct
    )
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(TakeAttemptSql, conn);
        cmd.Parameters.AddWithValue("@user_id", userId);
        cmd.Parameters.AddWithValue("@max_attempts", maxAttempts);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct)
            ? (reader.GetGuid(0), reader.GetFieldValue<byte[]>(1))
            : null;
    }

    public async Task<bool> ConsumeAsync(Guid challengeId, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(ConsumeSql, conn);

        cmd.Parameters.AddWithValue("@challenge_id", challengeId);

        return await cmd.ExecuteNonQueryAsync(ct) == 1;
    }

    public async Task<Guid?> ResendAsync(
        Guid userId, byte[] secretHash,
        DateTimeOffset expiresAt,
        TimeSpan minInterval,
        CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(ResendSql, conn);
        cmd.Parameters.AddWithValue("@user_id", userId);
        cmd.Parameters.AddWithValue("@secret_hash", secretHash);
        cmd.Parameters.AddWithValue("@expires_at", expiresAt);
        cmd.Parameters.AddWithValue("@min_interval", minInterval);

        return await cmd.ExecuteScalarAsync(ct) as Guid?;
    }
}
