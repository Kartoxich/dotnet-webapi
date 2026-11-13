using System.Text;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Register;

public class PasswordValidatorTests
{
    [Theory]
    [InlineData("cекрет12")]
    [InlineData("pass1234")]
    [InlineData("Пароль12345")]
    public void Validate_Accepts(string password)
        => Assert.Null(PasswordValidator.Validate(password));

    [Theory]
    [InlineData("abc")]
    [InlineData("12345678")]
    [InlineData("QWERTYUI")]
    public void Validate_Rejects(string password)
        => Assert.NotNull(PasswordValidator.Validate(password));

    [Fact]
    public void Validate_RejectsPasswordExceeding72Bytes()
    {
        var password = new string('а', 50) + "1";

        Assert.Equal(51, password.EnumerateRunes().Count());
        Assert.True(Encoding.UTF8.GetByteCount(password) > 72);

        Assert.NotNull(PasswordValidator.Validate(password));
    }
}
