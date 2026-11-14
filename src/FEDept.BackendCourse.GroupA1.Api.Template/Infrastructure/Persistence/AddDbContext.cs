using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Microsoft.EntityFrameworkCore;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RegistrationChallenge> RegistrationChallenges => Set<RegistrationChallenge>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.Property(user => user.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(user => user.Email).HasColumnName("email");
            e.Property(user => user.PasswordHash).HasColumnName("password_hash");
            e.Property(user => user.Status).HasColumnName("status")
                .HasDefaultValue(nameof(UserStatus.Pending).ToLowerInvariant());
            e.Property(user => user.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(user => user.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.ToTable("users", t => t.HasCheckConstraint(
                "ck_users_status", "status IN ('pending', 'active')")
            );
        });

        b.Entity<RegistrationChallenge>(e =>
        {
            e.ToTable("registration_challenges");
            e.Property(registrationChallenge => registrationChallenge.Id).HasColumnName("registration_challenge_id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(registrationChallenge => registrationChallenge.UserId).HasColumnName("user_id");
            e.Property(registrationChallenge => registrationChallenge.SecretHash).HasColumnName("secret_hash");
            e.Property(registrationChallenge => registrationChallenge.Method).HasColumnName("method").HasDefaultValue("otp");
            e.Property(registrationChallenge => registrationChallenge.Attempts).HasColumnName("attempts").HasDefaultValue(0);
            e.Property(registrationChallenge => registrationChallenge.ExpiresAt).HasColumnName("expires_at");
            e.Property(registrationChallenge => registrationChallenge.LastSentAt).HasColumnName("last_sent_at").HasDefaultValueSql("now()");
            e.Property(registrationChallenge => registrationChallenge.ConsumedAt).HasColumnName("consumed_at");

            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(registrationChallenge => registrationChallenge.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.ToTable("registration_challenges", rc => rc.HasCheckConstraint(
                "ck_challenges_method", "method IN ('otp', 'magic_link')"));
        });
    }
}
