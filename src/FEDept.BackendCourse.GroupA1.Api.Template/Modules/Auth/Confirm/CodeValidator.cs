namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public class CodeValidator
{
    private const int CodeDigitsCount = 6;
    public static string? Validate(string? code)
    {
        if (string.IsNullOrEmpty(code)) return "Code is required";
        if (!code.All(char.IsDigit)) return "Code must contain only digits";
        if (code.Length != CodeDigitsCount) return $"Code length must be exactly {CodeDigitsCount} digits";

        return null;
    }
}
