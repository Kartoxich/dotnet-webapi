using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;

public class CodeOptionsTests
{
    private static IConfiguration Config(int max, int perMinute) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Code:MaxAttempts"] = max.ToString(),
            ["Code:PerMinute"] = perMinute.ToString(),
            ["Code:TtlMinutes"] = "10",
            ["Code:Key"] = "test-key-0123456789abcdef",
            ["Resend:CooldownSeconds"] = "60",
            ["Resend:PerMinute"] = "5",
        }).Build();

    private static CodeOptions Bind(IConfiguration config)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAppOptions(config);
        return services.BuildServiceProvider()
            .GetRequiredService<IOptions<CodeOptions>>().Value;
    }

    [Fact]
    public void Bind_MaxAttemptsBelowPerMinute_Succeeds()
        => Assert.Equal(5, Bind(Config(5, 10)).MaxAttempts);

    [Fact]
    public void Bind_MaxAttemptsEqualToPerMinute_ThrowsBecauseLockoutIsUnreachable()
    {
        OptionsValidationException ex = Assert.Throws<OptionsValidationException>(() => Bind(Config(10, 10)));
        Assert.Contains("unreachable", ex.Message);
    }

    [Fact]
    public void Bind_MaxAttemptsAbovePerMinute_Throws()
        => Assert.Throws<OptionsValidationException>(() => Bind(Config(11, 10)));

    [Fact]
    public void Bind_MissingCodeKey_ThrowsAndNamesTheKey()
    {
        IConfigurationRoot config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Code:MaxAttempts"] = "5", ["Code:PerMinute"] = "10",
            ["Code:TtlMinutes"] = "10",
            ["Resend:CooldownSeconds"] = "60", ["Resend:PerMinute"] = "5",
        }).Build();

        OptionsValidationException ex = Assert.Throws<OptionsValidationException>(() => Bind(config));
        Assert.Contains("Code:Key", ex.Message);
    }
}
