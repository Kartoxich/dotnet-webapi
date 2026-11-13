using Npgsql;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public class UserRepository(string connectionString)
{
    private const string InsertSql = """
        INSERT INTO users (email, password_hash, status)
        VALUES (@email, @hash, @status)
        RETURNING id
    """;

    public async Task<Guid> CreateAsync(
        string email, string passwordHash, UserStatus status, CancellationToken ct
    )
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(InsertSql, connection);

        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@hash", passwordHash);
        cmd.Parameters.AddWithValue("@status", status.ToString().ToLowerInvariant());

        return (Guid)((await cmd.ExecuteScalarAsync(ct)) ?? throw new InvalidOperationException());
    }
}
