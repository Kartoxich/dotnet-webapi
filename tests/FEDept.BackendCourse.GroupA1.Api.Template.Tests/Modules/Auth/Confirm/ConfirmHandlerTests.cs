using System.Text;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Confirm;

public class ConfirmHandlerTests
{
    private const string CodeKey = "test-key-0123456789abcdef";
    private const string CorrectCode = "111111";
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid AttemptId = Guid.NewGuid();

    private static readonly byte[] CorrectHash =
        CodeProtector.Protect(CorrectCode, Encoding.UTF8.GetBytes(CodeKey));

    private static AttemptInfo Live(int attempts) => new(AttemptId, UserId, CorrectHash, attempts);

    private static readonly IOptions<CodeOptions> Options = Microsoft.Extensions.Options.Options.Create(
        new CodeOptions { MaxAttempts = 5, PerMinute = 10, TtlMinutes = 10, Key = CodeKey });

    private static ConfirmHandler Build(FakeUsers u, FakeChallenges c, FakeUnitOfWork w) =>
        new(u, c, w, Options, NullLogger<ConfirmHandler>.Instance);

    [Fact]
    public async Task ConfirmAsync_CorrectCode_ActivatesTheAttemptOwnerAndSavesExactlyOnce()
    {
        var u = new FakeUsers();
        var w = new FakeUnitOfWork();

        var r = await Build(u, new FakeChallenges { Attempt = Live(1) }, w)
            .ConfirmAsync(AttemptId, CorrectCode, default);

        Assert.Equal(ConfirmOutcome.Success, r.Outcome);
        Assert.Equal(UserId, u.ActivatedId);
        Assert.Equal(1, w.SaveCount);
    }

    [Fact]
    public async Task ConfirmAsync_FindsTheAttemptByIdAndNeverLooksUpByEmail()
    {
        var u = new FakeUsers();
        var c = new FakeChallenges { Attempt = Live(1) };

        await Build(u, c, new FakeUnitOfWork()).ConfirmAsync(AttemptId, CorrectCode, default);

        Assert.Equal(AttemptId, c.LastAttemptId);
        Assert.False(u.FindCalled);
    }

    [Fact]
    public async Task ConfirmAsync_WrongCode_SpendsAttemptAndActivatesNothing()
    {
        var u = new FakeUsers();
        var w = new FakeUnitOfWork();

        var r = await Build(u, new FakeChallenges { Attempt = Live(1) }, w)
            .ConfirmAsync(AttemptId, "222222", default);

        Assert.Equal(ConfirmOutcome.InvalidCode, r.Outcome);
        Assert.Null(u.ActivatedId);
        Assert.Equal(0, w.SaveCount);
    }

    [Fact]
    public async Task ConfirmAsync_AttemptsExactlyAtLimit_IsLockedOut()
    {
        var c = new FakeChallenges { Attempt = Live(5) };

        var r = await Build(new FakeUsers(), c, new FakeUnitOfWork())
            .ConfirmAsync(AttemptId, CorrectCode, default);

        Assert.Equal(ConfirmOutcome.AttemptsExhausted, r.Outcome);
        Assert.False(c.MarkConsumedCalled);
    }

    [Fact]
    public async Task ConfirmAsync_WrongCodeAfterLockoutReached_DoesNotChargeAgain()
    {
        var c = new FakeChallenges { Attempt = Live(6) };

        var r = await Build(new FakeUsers(), c, new FakeUnitOfWork())
            .ConfirmAsync(AttemptId, CorrectCode, default);

        Assert.Equal(ConfirmOutcome.AttemptsExhausted, r.Outcome);
        Assert.Equal(0, c.WrongAttemptCalls);
        Assert.False(c.MarkConsumedCalled);
    }

    [Fact]
    public async Task ConfirmAsync_WrongCode_SpendsExactlyOneAttempt()
    {
        var c = new FakeChallenges { Attempt = Live(1) };

        var r = await Build(new FakeUsers(), c, new FakeUnitOfWork())
            .ConfirmAsync(AttemptId, "222222", default);

        Assert.Equal(ConfirmOutcome.InvalidCode, r.Outcome);
        Assert.Equal(1, c.WrongAttemptCalls);
        Assert.Equal(5, c.LastMaxAttempts);
        Assert.False(c.MarkConsumedCalled);
    }

    [Fact]
    public async Task ConfirmAsync_FifthWrongInput_ConsumesAndAnswers429()
    {
        var c = new FakeChallenges { Attempt = Live(4) };

        var r = await Build(new FakeUsers(), c, new FakeUnitOfWork())
            .ConfirmAsync(AttemptId, "222222", default);

        Assert.Equal(ConfirmOutcome.AttemptsExhausted, r.Outcome);
        Assert.Equal(1, c.WrongAttemptCalls);
    }

    [Fact]
    public async Task ConfirmAsync_CorrectCode_SpendsNoAttempt()
    {
        var c = new FakeChallenges { Attempt = Live(1) };

        var r = await Build(new FakeUsers(), c, new FakeUnitOfWork())
            .ConfirmAsync(AttemptId, CorrectCode, default);

        Assert.Equal(ConfirmOutcome.Success, r.Outcome);
        Assert.Equal(0, c.WrongAttemptCalls);
    }

    [Fact]
    public async Task ConfirmAsync_ChargeLost_ReturnsInvalidCode()
    {
        var c = new FakeChallenges { Attempt = Live(1), ChargeResult = false };

        var r = await Build(new FakeUsers(), c, new FakeUnitOfWork())
            .ConfirmAsync(AttemptId, "222222", default);

        Assert.Equal(ConfirmOutcome.InvalidCode, r.Outcome);
    }

    [Fact]
    public async Task ConfirmAsync_AlreadyConsumed_ReturnsInvalidCode()
    {
        var w = new FakeUnitOfWork();

        var r = await Build(new FakeUsers(),
            new FakeChallenges { Attempt = Live(1), ConsumeResult = false }, w)
            .ConfirmAsync(AttemptId, CorrectCode, default);

        Assert.Equal(ConfirmOutcome.InvalidCode, r.Outcome);
        Assert.Equal(0, w.SaveCount);
    }

    [Fact]
    public async Task ConfirmAsync_UnknownAttempt_ReturnsInvalidCode()
    {
        var c = new FakeChallenges { Attempt = null };

        var r = await Build(new FakeUsers(), c, new FakeUnitOfWork())
            .ConfirmAsync(Guid.NewGuid(), CorrectCode, default);

        Assert.Equal(ConfirmOutcome.InvalidCode, r.Outcome);
        Assert.Equal(0, c.WrongAttemptCalls);
        Assert.False(c.MarkConsumedCalled);
    }

    [Fact]
    public async Task ConfirmAsync_NoLiveChallenge_ReturnsInvalidCode_AndSpendsNothing()
    {
        var c = new FakeChallenges { Attempt = null };

        var r = await Build(new FakeUsers(), c, new FakeUnitOfWork())
            .ConfirmAsync(AttemptId, CorrectCode, default);

        Assert.Equal(ConfirmOutcome.InvalidCode, r.Outcome);
        Assert.Equal(0, c.WrongAttemptCalls);
        Assert.False(c.MarkConsumedCalled);
    }

    [Fact]
    public async Task ConfirmAsync_MalformedCode_IsInvalidRequest_AndNeverTouchesDatabase()
    {
        var c = new FakeChallenges { Attempt = Live(1) };

        var r = await Build(new FakeUsers(), c, new FakeUnitOfWork())
            .ConfirmAsync(AttemptId, "12ab6", default);

        Assert.Equal(ConfirmOutcome.InvalidRequest, r.Outcome);
        Assert.NotNull(r.Errors);
        Assert.Null(c.LastAttemptId);
    }

    [Fact]
    public async Task ConfirmAsync_MissingAttemptId_IsInvalidRequest_AndNeverTouchesDatabase()
    {
        var c = new FakeChallenges { Attempt = Live(1) };

        var r = await Build(new FakeUsers(), c, new FakeUnitOfWork())
            .ConfirmAsync(null, CorrectCode, default);

        Assert.Equal(ConfirmOutcome.InvalidRequest, r.Outcome);
        Assert.NotNull(r.Errors);
        Assert.Contains("attemptId", r.Errors.Keys);
        Assert.Null(c.LastAttemptId);
    }

    [Fact]
    public async Task ConfirmAsync_EmptyAttemptId_IsInvalidRequest()
    {
        var r = await Build(new FakeUsers(), new FakeChallenges { Attempt = Live(1) },
                new FakeUnitOfWork())
            .ConfirmAsync(Guid.Empty, CorrectCode, default);

        Assert.Equal(ConfirmOutcome.InvalidRequest, r.Outcome);
        Assert.Contains("attemptId", r.Errors!.Keys);
    }
}
