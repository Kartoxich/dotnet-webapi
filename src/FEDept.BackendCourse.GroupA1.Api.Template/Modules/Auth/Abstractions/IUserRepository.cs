namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public interface IUserRepository
{
    Task<Guid> CreateAsync(string email, string passwordHash, CancellationToken ct);
    Task<Guid?> FindIdByEmailAsync(string email, CancellationToken ct);
    Task<bool> TryMarkActiveAsync(Guid id, CancellationToken ct);
}
