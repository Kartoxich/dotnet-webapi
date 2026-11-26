using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Resend;
using FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth;

public class ResendHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly IOptions<CodeOptions> Options = Microsoft.Extensions.Options.Options.Create(
        new CodeOptions
        {
            MaxAttempts = 5, PerMinute = 10,
            TtlMinutes = 10, Key = "test-key-0123456789abcdef",
        });

    private static readonly IOptions<ResendOptions> ResendOpts = Microsoft.Extensions.Options.Options.Create(
        new ResendOptions { CooldownSeconds = 60, PerMinute = 5 });

    private static ResendHandler Build(
        FakeUsers u, FakeChallenges c, FakeEmailSender? mail = null) =>
        new(u, c, mail ?? new FakeEmailSender(), Options, ResendOpts, NullLogger<ResendHandler>.Instance);

    [Fact]
    public async Task ResendAsync_Issued_MailsExactlyOnceWithSixDigits()
    {
        var mail = new FakeEmailSender();
        ResendResult r = await Build(new FakeUsers { IdByEmail = UserId }, new FakeChallenges(), mail)
            .ResendAsync("User@Example.com", CancellationToken.None);

        Assert.Equal(ResendOutcome.Accepted, r.Outcome);
        Assert.Single(mail.Sent);
        Assert.Equal("user@example.com", mail.Sent[0].Email);
        Assert.Matches("^[0-9]{6}$", mail.Sent[0].Secret);
        Assert.Equal(ChallengeMethod.Otp, mail.Sent[0].Method);
        Assert.Equal(UserId, mail.Sent[0].AttemptId);
        Assert.True(mail.Sent[0].ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ResendAsync_Throttled_AcceptsButSendsNoMail()
    {
        var mail = new FakeEmailSender();
        ResendResult r = await Build(new FakeUsers { IdByEmail = UserId },
            new FakeChallenges { IssueResult = CodeIssue.Throttled }, mail)
            .ResendAsync("user@example.com", CancellationToken.None);

        Assert.Equal(ResendOutcome.Accepted, r.Outcome);
        Assert.Empty(mail.Sent);
    }

    [Fact]
    public async Task ResendAsync_UnknownOrAlreadyActive_SendsNothing()
    {
        var mail = new FakeEmailSender();
        ResendResult r = await Build(new FakeUsers { IdByEmail = UserId, Pending = false },
            new FakeChallenges(), mail).ResendAsync("ghost@example.com", default);

        Assert.Equal(ResendOutcome.Accepted, r.Outcome);
        Assert.Empty(mail.Sent);
    }

    [Fact]
    public async Task ResendAsync_MailFails_StillAcceptsSoTheUserCanRetry()
    {
        ResendResult r = await new ResendHandler(new FakeUsers { IdByEmail = UserId },
            new FakeChallenges(), new BrokenEmailSender(), Options, ResendOpts,
            NullLogger<ResendHandler>.Instance).ResendAsync("user@example.com", default);

        Assert.Equal(ResendOutcome.Accepted, r.Outcome);
    }

    [Fact]
    public async Task ResendAsync_BlankEmail_IsInvalidRequest_WithoutAnyIo()
    {
        var u = new FakeUsers { IdByEmail = UserId };
        ResendResult r = await Build(u, new FakeChallenges()).ResendAsync("   ", default);

        Assert.Equal(ResendOutcome.InvalidRequest, r.Outcome);
        Assert.NotNull(r.Errors);
        Assert.False(u.FindCalled);
    }
}
