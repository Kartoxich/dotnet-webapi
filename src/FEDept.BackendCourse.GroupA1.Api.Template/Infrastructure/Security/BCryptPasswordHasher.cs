using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Security;

public class BCryptPasswordHasher(IConfiguration config): IPasswordHasher
{
    private readonly int _workFactor = config.GetValue<int?>("Security:WorkFactor") ?? 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password,  _workFactor);
}
