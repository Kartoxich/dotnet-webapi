using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Register;

public class RegisterValidatorTests
{
    private const string ValidEmail = "a@b.c";
    private const string ValidPassword = "секрет12";

    [Theory]
    [InlineData(ValidEmail, ValidPassword)]
    [InlineData("user+tag@x.com", ValidPassword)]
    public void Validate_Accept(string? email, string? password)
        => Assert.Null(RegisterValidator.Validate(email, password));

    [Theory]
    [InlineData(ValidEmail, null, "password")]
    [InlineData(null, ValidPassword, "email")]
    public void Validate_RejectsOnlyNamedField(string? email, string? password, string expectedKey)
    {
        IDictionary<string, string[]>? errors = RegisterValidator.Validate(email, password);

        Assert.NotNull(errors);
        Assert.True(errors.ContainsKey(expectedKey));
        Assert.Single(errors);
    }

    [Fact]
    public void Validate_CollectsErrorsForBothFields()
    {
        IDictionary<string, string[]>? errors = RegisterValidator.Validate("no-at-sign", "abc");

        Assert.NotNull(errors);
        Assert.Equal(2, errors.Count);
        Assert.True(errors.ContainsKey("email"));
        Assert.True(errors.ContainsKey("password"));
    }
}
