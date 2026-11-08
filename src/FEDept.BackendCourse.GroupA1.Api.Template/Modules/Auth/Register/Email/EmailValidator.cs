using System.Text.RegularExpressions;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

public static partial class EmailValidator
{
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    [GeneratedRegex(EmailPattern, RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    public static string Normalize(string email) => email.Trim().ToLowerInvariant();

    public static string? Validate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required";
        }

        if (!EmailRegex().IsMatch(email))
        {
            return "Email is not valid";
        };

        return null;
    }
}
