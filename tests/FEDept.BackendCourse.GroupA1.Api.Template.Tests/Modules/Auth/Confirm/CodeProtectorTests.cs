using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Domain;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Confirm;

public class CodeProtectorTests
{
    [Fact]
    public void Protect_IsStableAndMatchable()
    {
        var key = new byte[32];
        var (code, _) = ConfirmationCode.New(key);
        Assert.True(CodeProtector.Matches(code, key, CodeProtector.Protect(code, key)));
        Assert.False(CodeProtector.Matches("000000", key, CodeProtector.Protect(code, key)));
    }
}
