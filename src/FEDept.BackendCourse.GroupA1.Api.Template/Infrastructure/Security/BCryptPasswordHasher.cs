using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Security;

public sealed class BCryptPasswordHasher(IOptions<SecurityOptions> options) : IPasswordHasher
{
    private readonly int _workFactor = options.Value.WorkFactor;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
}
