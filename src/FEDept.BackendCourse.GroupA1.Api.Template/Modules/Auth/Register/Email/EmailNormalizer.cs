using System.Globalization;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

public static class EmailNormalizer
{
    private static readonly IdnMapping IdnMapper = new();

    public static string Normalize(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return string.Empty;
        }

        var cleanEmail = email.Trim().ToLowerInvariant();
        var parts = cleanEmail.Split('@', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            return cleanEmail.ToLowerInvariant();
        }

        var localPart = parts[0];
        var domainPart = parts[1];

        try
        {
            domainPart = IdnMapper.GetAscii(domainPart);
        }
        catch (ArgumentException)
        {
        }

        return $"{localPart}@{domainPart}";
    }
}
