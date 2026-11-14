using System.Reflection;
using System.Text.RegularExpressions;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Persistence;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Infrastructure.Persistence;

public class SchemaDriftTests
{
    private static readonly string[] SqlNoise =
    [
        "INSERT", "INTO", "VALUES", "RETURNING", "SELECT", "FROM", "WHERE",
        "UPDATE", "SET", "AND", "OR", "NOT", "NULL", "IS", "ORDER", "BY", "LIMIT",
        "lower", "now", "gen_random_uuid", "count"
    ];

    [Fact]
    public void RawSqlReferencesOnlyColumnsFromEfModel()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=drift_check;Username=x;Password=y")
            .Options;
        using var context = new AppDbContext(options);

        IEntityType entityType = context.Model.FindEntityType(typeof(User))!;
        var tableName = entityType.GetTableName()!;
        var known = entityType.GetProperties()
            .Select(p => p.GetColumnName())
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            new HashSet<string>(StringComparer.Ordinal)
            {
                "id", "email", "password_hash", "status", "created_at", "updated_at"
            },
            known);
        Assert.NotEmpty(known);

        var sql = typeof(UserRepository)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();

        Assert.NotEmpty(sql);

        var offenders = sql
            .SelectMany(s => Regex.Matches(Regex.Replace(
                s,
                @"@\w+",
                " "),
                "[A-Za-z_][A-Za-z0-9_]*"))
            .Select(m => m.Value)
            .Where(t => !known.Contains(t)
                     && !t.Equals(tableName, StringComparison.Ordinal)
                     && !SqlNoise.Contains(t, StringComparer.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

        Assert.Empty(offenders);
        Assert.Equal("users", tableName);
    }


    [Fact] public void UserSqlMatchesModel()
        => Check(typeof(User), typeof(UserRepository), "users");

    [Fact] public void ChallengeSqlMatchesModel()
        => Check(typeof(RegistrationChallenge), typeof(ChallengeRepository), "registration_challenges");

    [Fact]
    public void EveryChallengeMethodHasADistinctColumnValue()
        => Assert.Equal(["otp", "magic_link"],
            Enum.GetValues<ChallengeMethod>().Select(m => m.ToColumn()));

    private static void Check(Type entity, Type repository, string expectedTable)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=drift;Username=x;Password=y").Options;
        using var ctx = new AppDbContext(options);

        var all = ctx.Model.GetEntityTypes().ToList();
        var tables = all.Select(t => t.GetTableName()!).ToHashSet(StringComparer.Ordinal);
        var mine = all.Single(t => t.ClrType == entity);

        Assert.Equal(expectedTable, mine.GetTableName());
        var known = mine.GetProperties().Select(p => p.GetColumnName())
            .ToHashSet(StringComparer.Ordinal);
        Assert.NotEmpty(known);

        var sql = repository.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();
        Assert.NotEmpty(sql);

        var offenders = sql
            .SelectMany(s => Regex.Matches(
                Regex.Replace(Regex.Replace(s, @"@\w+", " "), @"'[^']*'", " "),
                "[A-Za-z_][A-Za-z0-9_]*"))
            .Select(m => m.Value)
            .Where(t => !known.Contains(t) && !tables.Contains(t)
                                           && !SqlNoise.Contains(t, StringComparer.OrdinalIgnoreCase))
            .Distinct().ToList();

        Assert.Empty(offenders);
    }
}
