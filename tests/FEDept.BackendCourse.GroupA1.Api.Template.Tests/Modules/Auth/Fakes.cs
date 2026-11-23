namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth;

using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

internal sealed class FakeUsers : IUserRepository
{
    public Guid? IdByEmail { get; init; }
    public bool Pending { get; init; } = true;
    public bool FindCalled { get; private set; }
    public Guid? ActivatedId { get; private set; }

    public Task<Guid> CreateAsync(string e, string p, CancellationToken ct)
        => throw new NotSupportedException("confirm/resend must never create users");
    public Task<Guid?> FindIdByEmailAsync(string e, CancellationToken ct)
    { FindCalled = true; return Task.FromResult(IdByEmail); }
    public Task<Guid?> FindPendingIdByEmailAsync(string e, CancellationToken ct)
    { FindCalled = true; return Task.FromResult(Pending ? IdByEmail : null); }
    public Task<bool> TryMarkActiveAsync(Guid id, CancellationToken ct)
    { ActivatedId = id; return Task.FromResult(true); }
}

internal sealed class FakeChallenges : IChallengeRepository
{
    public AttemptInfo? Attempt { get; init; }
    public CodeIssue IssueResult { get; init; } = CodeIssue.Issued;
    public bool ConsumeResult { get; init; } = true;
    public bool MarkConsumedCalled { get; private set; }
    public int IssueCalls { get; private set; }
    public TimeSpan? LastCooldown { get; private set; }

    public Task<CodeIssue> IssueAsync(Guid u, byte[] h, DateTimeOffset e, TimeSpan c, CancellationToken ct)
    { IssueCalls++; LastCooldown = c; return Task.FromResult(IssueResult); }

    public Task<AttemptInfo?> TakeAttemptAsync(Guid u, CancellationToken ct)
        => Task.FromResult(Attempt);

    public Task<bool> TryMarkConsumedAsync(Guid id, CancellationToken ct)
    { MarkConsumedCalled = true; return Task.FromResult(ConsumeResult); }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    { SaveCount++; return Task.FromResult(1); }
}

internal sealed class FakeEmailSender : IEmailSender
{
    public List<(string To, string Code)> Sent { get; } = new();
    public Task SendConfirmationAsync(string to, string code, CancellationToken ct)
    { Sent.Add((to, code)); return Task.CompletedTask; }
}

internal sealed class BrokenEmailSender : IEmailSender
{
    public Task SendConfirmationAsync(string to, string code, CancellationToken ct)
        => throw new InvalidOperationException("mailpit is down");
}

internal sealed class NoopEmailSender : IEmailSender
{
    public Task SendConfirmationAsync(string to, string code, CancellationToken ct)
        => Task.CompletedTask;
}
