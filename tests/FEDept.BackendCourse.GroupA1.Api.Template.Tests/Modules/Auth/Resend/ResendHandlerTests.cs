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
        FakeUsers u, FakeChallenges c, FakeEmailSender? mail = null, FakeUnitOfWork? w = null) =>
        new(u, c, mail ?? new FakeEmailSender(), w ?? new FakeUnitOfWork(),
            Options, ResendOpts, NullLogger<ResendHandler>.Instance);

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
        Assert.Equal(TimeSpan.FromSeconds(60), mail.Sent[0].ResendAfter);
        Assert.True(mail.Sent[0].ExpiresAt > DateTimeOffset.UtcNow);

        ConfirmationTicket ticket = r.Ticket!;
        Assert.Equal(mail.Sent[0].AttemptId, ticket.AttemptId);
        Assert.Equal(TimeSpan.FromMinutes(10), ticket.Ttl);
        Assert.Equal(TimeSpan.FromSeconds(60), ticket.ResendAfter);
        Assert.DoesNotContain(mail.Sent[0].Secret,
            System.Text.Json.JsonSerializer.Serialize(ticket));
    }

    [Fact]
    public async Task ResendAsync_Throttled_AcceptsButSendsNoMail()
    {
        var mail = new FakeEmailSender();
        ResendResult r = await Build(new FakeUsers { IdByEmail = UserId },
            new FakeChallenges { IssueResult = CodeIssue.Throttled }, mail)
            .ResendAsync("user@example.com", CancellationToken.None);

        Assert.Equal(ResendOutcome.Accepted, r.Outcome);
        Assert.Null(r.Ticket);
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
    public async Task ResendAsync_MailFails_ReturnsMailFailed()
    {
        ResendResult r = await new ResendHandler(new FakeUsers { IdByEmail = UserId },
            new FakeChallenges(), new BrokenEmailSender(), new FakeUnitOfWork(),
            Options, ResendOpts, NullLogger<ResendHandler>.Instance)
            .ResendAsync("user@example.com", default);

        Assert.Equal(ResendOutcome.MailFailed, r.Outcome);
    }

    [Fact]
    public async Task ResendAsync_MailSucceeded_SavesOnce()
    {
        var w = new FakeUnitOfWork();

        ResendResult r = await Build(new FakeUsers { IdByEmail = UserId }, new FakeChallenges(), w: w)
            .ResendAsync("user@example.com", default);

        Assert.Equal(ResendOutcome.Accepted, r.Outcome);
        Assert.Equal(1, w.SaveCount);
    }

    [Fact]
    public async Task ResendAsync_Throttled_SavesNothing()
    {
        var w = new FakeUnitOfWork();

        await Build(new FakeUsers { IdByEmail = UserId },
            new FakeChallenges { IssueResult = CodeIssue.Throttled }, w: w)
            .ResendAsync("user@example.com", default);

        Assert.Equal(0, w.SaveCount);
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
