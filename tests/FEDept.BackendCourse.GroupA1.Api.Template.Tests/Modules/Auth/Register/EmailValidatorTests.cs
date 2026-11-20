using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Register.Email;

public class EmailValidatorTests
{
    [Theory]
    [InlineData("a@b.c")]
    [InlineData("a@example.com")]
    [InlineData("email@x.com")]
    public void Validate_Accepts(string? email)
        => Assert.Null(EmailValidator.Validate(email));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a@b.c\n")]
    [InlineData("example.com")]
    [InlineData("@example.com")]
    [InlineData("user@localhost")]
    [InlineData("two@@example.com")]
    [InlineData("user name@example.com")]
    public void Validate_Rejects(string? email)
        => Assert.NotNull(EmailValidator.Validate(email));
}
