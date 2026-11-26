using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Register;

public class RegisterHandlerTests
{
    private static readonly IOptions<CodeOptions> Options = Microsoft.Extensions.Options.Options.Create(
        new CodeOptions
        {
            MaxAttempts = 5, PerMinute = 10,
            TtlMinutes = 10, Key = "test-key-0123456789abcdef",
        });

    private static RegisterHandler Build(
        FakeUsers u, FakeEmailSender mail, FakeChallenges? c = null, FakeUnitOfWork? w = null) =>
        new(u, c ?? new FakeChallenges(), new FakePasswordHasher(), mail,
            w ?? new FakeUnitOfWork(), Options, NullLogger<RegisterHandler>.Instance);

    [Fact]
    public async Task RegisterAsync_MailsOneMessageCarryingEverythingTheSenderNeeds()
    {
        var mail = new FakeEmailSender();
        var u = new FakeUsers();

        RegisterResult r = await Build(u, mail)
            .RegisterAsync("User@Example.com", "secret12", default);

        Assert.Equal(RegisterOutcome.Success, r.Outcome);
        Assert.Single(mail.Sent);

        ConfirmationMessage sent = mail.Sent[0];
        Assert.Equal("user@example.com", sent.Email);
        Assert.Equal(ChallengeMethod.Otp, sent.Method);
        Assert.Matches("^[0-9]{6}$", sent.Secret);
        Assert.NotEqual(Guid.Empty, sent.AttemptId);
        Assert.Equal(TimeSpan.FromMinutes(10), sent.Ttl);
        Assert.True(sent.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsATicketWithoutTheSecret()
    {
        var mail = new FakeEmailSender();
        var u = new FakeUsers();

        RegisterResult r = await Build(u, mail)
            .RegisterAsync("user@example.com", "secret12", default);

        ConfirmationTicket ticket = Assert.IsType<ConfirmationTicket>(r.Ticket);
        Assert.Equal(mail.Sent[0].AttemptId, ticket.AttemptId);
        Assert.Equal(mail.Sent[0].Method, ticket.Method);
        Assert.Equal(mail.Sent[0].ExpiresAt, ticket.ExpiresAt);

        Assert.DoesNotContain(mail.Sent[0].Secret,
            System.Text.Json.JsonSerializer.Serialize(ticket));
    }

    [Fact]
    public async Task RegisterAsync_MailFails_ReturnsMailFailedAndKeepsTheUserPending()
    {
        var u = new FakeUsers();
        var w = new FakeUnitOfWork();

        RegisterResult r = await new RegisterHandler(
                u, new FakeChallenges(), new FakePasswordHasher(), new BrokenEmailSender(),
                w, Options, NullLogger<RegisterHandler>.Instance)
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

        Assert.Equal(RegisterOutcome.Success, r.Outcome);
        Assert.Equal(1, w.SaveCount);
    }
}
