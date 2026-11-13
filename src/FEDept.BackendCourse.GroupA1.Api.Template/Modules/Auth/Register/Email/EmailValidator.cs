using System.Text.RegularExpressions;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

public static partial class EmailValidator
{
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+\z";

    [GeneratedRegex(EmailPattern, RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    public static string? Validate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required";
        }

        return !EmailRegex().IsMatch(email)
            ? "Email is not valid"
            : null;
    }
}
