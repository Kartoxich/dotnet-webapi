using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Persistence;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Microsoft.EntityFrameworkCore;

public class UserRepository(AppDbContext db)
{
    public async Task<Guid> CreateAsync(
        string email,
        string passwordHash,
        CancellationToken ct
    )
    {
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Status = UserStatus.Pending
        };

        db.Users.Add(user);

        await db.SaveChangesAsync(ct);

        return user.Id;
    }

    public async Task<Guid?> FindIdByEmailAsync(string email,  CancellationToken ct)
    {
        Guid userId = await db.Users.AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);

        return userId == Guid.Empty ? null : userId;
    }

    public async Task<bool> TryMarkActiveAsync(Guid id, CancellationToken ct)
    {
        User? u = await db.Users.FirstOrDefaultAsync(x => x.Id == id && x.Status == UserStatus.Pending, ct);
        if (u is null)
        {
            return false;
        }

        u.Status = UserStatus.Active;
        return true;
    }
}
