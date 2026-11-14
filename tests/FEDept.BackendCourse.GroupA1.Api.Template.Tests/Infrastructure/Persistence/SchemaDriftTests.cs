using System.Reflection;
using System.Text.RegularExpressions;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Persistence;
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
        "UPDATE", "SET", "AND", "OR", "NOT", "NULL", "ORDER", "BY", "LIMIT",
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
}
