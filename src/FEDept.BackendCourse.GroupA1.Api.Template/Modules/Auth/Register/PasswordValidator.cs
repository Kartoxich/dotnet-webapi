using System.Text;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public static class PasswordValidator
{
    private const int MinChars = 8;
    private const int MaxBytes = 72;

    public static string? Validate(string? password)
    {
        if (string.IsNullOrEmpty(password)) return "Password is required";
        if (password.EnumerateRunes().Count() < MinChars) return $"Password length should be at least {MinChars} characters";
        if (Encoding.UTF8.GetByteCount(password) > MaxBytes) return "Password is too long";
        if (!password.Any(char.IsLetter)) return "Password must contain at least 1 letter";
        if (!password.Any(char.IsDigit)) return "Password must contain at least 1 digit";

        return null;
    }
}
