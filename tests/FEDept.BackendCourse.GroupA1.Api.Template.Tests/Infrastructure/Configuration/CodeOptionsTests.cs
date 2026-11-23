using Microsoft.Extensions.Configuration;
using Xunit;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;

public class CodeOptionsTests
{
    private static IConfiguration Config(int max, int perMinute) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Code:MaxAttempts"] = max.ToString(),
            ["Code:PerMinute"] = perMinute.ToString(),
            ["Code:ResendPerMinute"] = "5",
            ["Code:CodeTtlMinutes"] = "10",
            ["Code:ResendCooldownSeconds"] = "60",
            ["Code:CodeKey"] = "test-key-0123456789abcdef",
        }).Build();

    [Fact]
    public void From_MaxAttemptsBelowPerMinute_Succeeds()
        => Assert.Equal(5, CodeOptions.From(Config(5, 10)).MaxAttempts);

    [Fact]
    public void From_MaxAttemptsEqualToPerMinute_ThrowsBecauseLockoutIsUnreachable()
    {
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => CodeOptions.From(Config(10, 10)));
        Assert.Contains("unreachable", ex.Message);
    }

    [Fact]
    public void From_MaxAttemptsAbovePerMinute_Throws()
        => Assert.Throws<InvalidOperationException>(() => CodeOptions.From(Config(11, 10)));

    [Fact]
    public void From_MissingCodeKey_ThrowsAndNamesTheKey()
    {
        IConfigurationRoot config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Code:MaxAttempts"] = "5", ["Code:PerMinute"] = "10",
            ["Code:ResendPerMinute"] = "5", ["Code:CodeTtlMinutes"] = "10",
            ["Code:ResendCooldownSeconds"] = "60",
        }).Build();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => CodeOptions.From(config));
        Assert.Contains("Code:CodeKey", ex.Message);
    }
}
