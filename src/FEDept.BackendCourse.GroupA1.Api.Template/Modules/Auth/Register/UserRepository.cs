using Npgsql;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public class UserRepository(string connectionString)
{
    private const string InsertSql = """
        INSERT INTO users (email, password_hash, status)
        VALUES (@email, @hash, @status)
        RETURNING id
    """;

    private const string GetIdByEmailSql = "SELECT id FROM users WHERE email = @email";

    private const string ActivateSql = "UPDATE users SET status = 'active' WHERE id = @id";

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

        return (Guid)(await cmd.ExecuteScalarAsync(ct) ?? throw new InvalidOperationException());
    }

    public async Task<Guid?> FindIdByEmailAsync(string email,  CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(GetIdByEmailSql, connection);

        cmd.Parameters.AddWithValue("@email", email);

        return await cmd.ExecuteScalarAsync(ct) as Guid?;
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(ActivateSql, connection);

        cmd.Parameters.AddWithValue("@id", id);

        await cmd.ExecuteScalarAsync(ct);
    }
}
