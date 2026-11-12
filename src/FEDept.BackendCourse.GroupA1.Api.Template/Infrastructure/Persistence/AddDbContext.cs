using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Microsoft.EntityFrameworkCore;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b) =>
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.Property(user => user.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(user => user.Email).HasColumnName("email");
            e.Property(user => user.PasswordHash).HasColumnName("password_hash");
            e.Property(user => user.Status).HasColumnName("status").HasDefaultValue(nameof(UserStatus.Pending).ToLowerInvariant());
            e.Property(user => user.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(user => user.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.ToTable("users", t => t.HasCheckConstraint(
                "ck_users_status", "status IN ('pending', 'active')")
            );
        });
}
