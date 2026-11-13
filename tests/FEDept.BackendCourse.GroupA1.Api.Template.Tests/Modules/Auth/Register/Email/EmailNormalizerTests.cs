using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Register.Email;

public class EmailNormalizerTests
{
    [Theory]
    [InlineData("")]
    [InlineData("\t")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Normalize_Whitespace(string? email)
        => Assert.Empty(EmailNormalizer.Normalize(email));

    [Fact]
    public void Normalize_ValidEmail()
    {
        const string email = " Test@Example.COM ";
        var normalizedEmail = EmailNormalizer.Normalize(email);

        Assert.Equal("test@example.com", normalizedEmail);
    }

    [Fact]
    public void Normalize_PreserveTag()
    {
        const string email = "User+Tag@X.CoM ";
        var normalizedEmail = EmailNormalizer.Normalize(email);

        Assert.Equal("user+tag@x.com", normalizedEmail);
    }
}
