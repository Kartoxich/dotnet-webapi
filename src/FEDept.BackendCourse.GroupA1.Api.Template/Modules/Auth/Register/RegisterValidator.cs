using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public static class RegisterValidator
{
    public static string? Validate(string? email, string? password)
    {
        return EmailValidator.Validate(email) ??
               PasswordValidator.Validate(password);
    }
}
