using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Register;

public class RegisterHandlerTests
{
    private static readonly IOptions<CodeOptions> Options = Microsoft.Extensions.Options.Options.Create(
        new CodeOptions
        {
            MaxAttempts = 5, PerMinute = 10,
            TtlMinutes = 10, Key = "test-key-0123456789abcdef",
        });

    private static readonly IOptions<ResendOptions> ResendOpts = Microsoft.Extensions.Options.Options.Create(
        new ResendOptions { CooldownSeconds = 60, PerMinute = 5 });

    private static IOptions<RegistrationOptions> RegistrationOpts(bool requireConfirmation) =>
        Microsoft.Extensions.Options.Options.Create(
            new RegistrationOptions { PerMinute = 5, RequireConfirmation = requireConfirmation });

    private static RegisterHandler Build(
        FakeUsers u,
        FakeEmailSender mail,
        FakeChallenges? c = null,
        FakeUnitOfWork? w = null,
        bool requireConfirmation = true) =>
        new(u, c ?? new FakeChallenges(), new FakePasswordHasher(), mail,
            w ?? new FakeUnitOfWork(), Options, ResendOpts,
            RegistrationOpts(requireConfirmation), NullLogger<RegisterHandler>.Instance);

    [Fact]
    public async Task RegisterAsync_MailsOneMessageCarryingEverythingTheSenderNeeds()
    {
        var mail = new FakeEmailSender();
        var u = new FakeUsers();

        RegisterResult r = await Build(u, mail)
            .RegisterAsync("User@Example.com", "secret12", default);

        Assert.Equal(RegisterOutcome.Pending, r.Outcome);
        Assert.Single(mail.Sent);

        ConfirmationMessage sent = mail.Sent[0];
        Assert.Equal("user@example.com", sent.Email);
        Assert.Equal(ChallengeMethod.Otp, sent.Method);
        Assert.Matches("^[0-9]{6}$", sent.Secret);
        Assert.NotEqual(Guid.Empty, sent.AttemptId);
        Assert.Equal(TimeSpan.FromMinutes(10), sent.Ttl);
        Assert.Equal(TimeSpan.FromSeconds(60), sent.ResendAfter);
        Assert.True(sent.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsATicketWithoutTheSecret()
    {
        var mail = new FakeEmailSender();

        RegisterResult r = await Build(new FakeUsers(), mail)
            .RegisterAsync("user@example.com", "secret12", default);

        ConfirmationTicket ticket = r.Ticket!;
        Assert.Equal(mail.Sent[0].AttemptId, ticket.AttemptId);
        Assert.Equal(mail.Sent[0].ExpiresAt, ticket.ExpiresAt);

        Assert.DoesNotContain(mail.Sent[0].Secret,
            System.Text.Json.JsonSerializer.Serialize(ticket));
    }

    [Fact]
    public async Task RegisterAsync_ConfirmationRequired_CreatesAPendingUser()
    {
        var u = new FakeUsers();

        RegisterResult r = await Build(u, new FakeEmailSender())
            .RegisterAsync("user@example.com", "secret12", default);

        Assert.Equal(RegisterOutcome.Pending, r.Outcome);
        Assert.Equal(UserStatus.Pending, u.CreatedStatus);
        Assert.Equal(UserStatus.Pending, r.Status);
    }

    [Fact]
    public async Task RegisterAsync_ConfirmationOff_CreatesAnActiveUserAndSendsNoMail()
    {
        var u = new FakeUsers();
        var mail = new FakeEmailSender();
        var challenges = new FakeChallenges();
        var w = new FakeUnitOfWork();

        RegisterResult r = await Build(u, mail, challenges, w, requireConfirmation: false)
            .RegisterAsync("user@example.com", "secret12", default);

        Assert.Equal(RegisterOutcome.Created, r.Outcome);
        Assert.Equal(u.CreatedId, r.Id);
        Assert.Equal("user@example.com", r.Email);
        Assert.Equal(UserStatus.Active, r.Status);
        Assert.Equal(UserStatus.Active, u.CreatedStatus);
        Assert.Null(r.Ticket);
        Assert.Empty(mail.Sent);
        Assert.Equal(0, challenges.IssueCalls);
        Assert.Equal(1, w.SaveCount);
    }

    [Fact]
    public async Task RegisterAsync_MailFails_ReturnsMailFailedAndKeepsTheUserPending()
    {
        var u = new FakeUsers();
        var w = new FakeUnitOfWork();

        RegisterResult r = await new RegisterHandler(
                u, new FakeChallenges(), new FakePasswordHasher(), new BrokenEmailSender(),
                w, Options, ResendOpts, RegistrationOpts(true), NullLogger<RegisterHandler>.Instance)
            .RegisterAsync("user@example.com", "secret12", default);

        Assert.Equal(RegisterOutcome.MailFailed, r.Outcome);
        Assert.Equal(UserStatus.Pending, r.Status);
        Assert.Null(r.Ticket);
        Assert.NotEqual(Guid.Empty, u.CreatedId);
        Assert.Equal(0, w.SaveCount);
    }

    [Fact]
    public async Task RegisterAsync_AfterMailSucceeded_SavesOnce()
    {
        var w = new FakeUnitOfWork();

        RegisterResult r = await Build(new FakeUsers(), new FakeEmailSender(), w: w)
            .RegisterAsync("user@example.com", "secret12", default);

        Assert.Equal(RegisterOutcome.Pending, r.Outcome);
        Assert.Equal(1, w.SaveCount);
    }
}
