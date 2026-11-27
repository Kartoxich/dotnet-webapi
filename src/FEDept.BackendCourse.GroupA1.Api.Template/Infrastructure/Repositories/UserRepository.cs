using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Persistence;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<Guid> CreateAsync(
        string email,
        string passwordHash,
        UserStatus status,
        CancellationToken ct
    )
    {
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Status = status
        };

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
                                           pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new EmailAlreadyTakenException(email);
        }

        return user.Id;
    }

    public async Task<Guid?> FindIdByEmailAsync(string email, CancellationToken ct)
    {
        Guid id = await db.Users.AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);

        return id == Guid.Empty ? null : id;
    }

    public async Task<bool> TryMarkActiveAsync(Guid id, CancellationToken ct)
    {
        User? user = await db.Users.FirstOrDefaultAsync(
            x => x.Id == id && x.Status == UserStatus.Pending, ct);

        if (user is null)
        {
            return false;
        }

        user.Status = UserStatus.Active;
        return true;
    }

    public async Task<Guid?> FindPendingIdByEmailAsync(string email, CancellationToken ct)
    {
        Guid id = await db.Users.AsNoTracking()
            .Where(u => u.Email == email && u.Status == UserStatus.Pending)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);

        return id == Guid.Empty ? null : id;
    }
}
