using System.Text;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Confirm;

public class ConfirmHandlerTests
{
    private const string CodeKey = "test-key-0123456789abcdef";
    private const string CorrectCode = "111111";
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ChallengeId = Guid.NewGuid();

    private static readonly byte[] CorrectHash =
        CodeProtector.Protect(CorrectCode, Encoding.UTF8.GetBytes(CodeKey));

    private static AttemptInfo Live(int attempts) => new(ChallengeId, CorrectHash, attempts);

    private static readonly CodeOptions Options = new()
    {
        MaxAttempts = 5,
        PerMinute = 10,
        CodeTtlMinutes = 10,
        CodeKey = CodeKey,
        ResendCooldownSeconds = 0,
        ResendPerMinute = 0,
    };

    private static ConfirmHandler Build(FakeUsers u, FakeChallenges c, FakeUnitOfWork w) =>
        new(u, c, w, Options, NullLogger<ConfirmHandler>.Instance);

    [Fact]
    public async Task ConfirmAsync_CorrectCode_ActivatesAndSavesExactlyOnce()
    {
        var u = new FakeUsers { IdByEmail = UserId };
        var w = new FakeUnitOfWork();

        var r = await Build(u, new FakeChallenges { Attempt = Live(1) }, w)
            .ConfirmAsync("user@example.com", CorrectCode, default);

        Assert.Equal(ConfirmOutcome.Success, r.Outcome);
        Assert.Equal(UserId, u.ActivatedId);
        Assert.Equal(1, w.SaveCount);
    }

    [Fact]
    public async Task ConfirmAsync_WrongCode_ChangesNothing()
    {
        var u = new FakeUsers { IdByEmail = UserId };
        var w = new FakeUnitOfWork();

        var r = await Build(u, new FakeChallenges { Attempt = Live(1) }, w)
            .ConfirmAsync("user@example.com", "222222", default);

        Assert.Equal(ConfirmOutcome.InvalidCode, r.Outcome);
        Assert.Null(u.ActivatedId);
        Assert.Equal(0, w.SaveCount);
    }

    [Fact]
    public async Task ConfirmAsync_AttemptsExactlyAtLimit_StillComparesCode()
    {
        var r = await Build(new FakeUsers { IdByEmail = UserId },
            new FakeChallenges { Attempt = Live(5) }, new FakeUnitOfWork())
            .ConfirmAsync("user@example.com", CorrectCode, default);

        Assert.Equal(ConfirmOutcome.Success, r.Outcome);
    }

    [Fact]
    public async Task ConfirmAsync_AttemptsOverLimit_DoesNotCompareCode()
    {
        var c = new FakeChallenges { Attempt = Live(6) };

        var r = await Build(new FakeUsers { IdByEmail = UserId }, c, new FakeUnitOfWork())
            .ConfirmAsync("user@example.com", CorrectCode, default);

        Assert.Equal(ConfirmOutcome.AttemptsExhausted, r.Outcome);
        Assert.False(c.MarkConsumedCalled);
    }

    [Fact]
    public async Task ConfirmAsync_AlreadyConsumed_ReturnsInvalidCode()
    {
        var w = new FakeUnitOfWork();

        var r = await Build(new FakeUsers { IdByEmail = UserId },
            new FakeChallenges { Attempt = Live(1), ConsumeResult = false }, w)
            .ConfirmAsync("user@example.com", CorrectCode, default);

        Assert.Equal(ConfirmOutcome.InvalidCode, r.Outcome);
        Assert.Equal(0, w.SaveCount);
    }

    [Fact]
    public async Task ConfirmAsync_UnknownEmail_ReturnsInvalidCode()
    {
        var r = await Build(new FakeUsers { IdByEmail = null },
            new FakeChallenges { Attempt = Live(1) }, new FakeUnitOfWork())
            .ConfirmAsync("ghost@example.com", "111111", default);

        Assert.Equal(ConfirmOutcome.InvalidCode, r.Outcome);
    }

    [Fact]
    public async Task ConfirmAsync_MalformedCode_IsInvalidRequest_AndNeverTouchesDatabase()
    {
        var u = new FakeUsers { IdByEmail = UserId };

        var r = await Build(u, new FakeChallenges { Attempt = Live(1) }, new FakeUnitOfWork())
            .ConfirmAsync("user@example.com", "12ab6", default);

        Assert.Equal(ConfirmOutcome.InvalidRequest, r.Outcome);
        Assert.NotNull(r.Errors);
        Assert.False(u.FindCalled);
    }
}
