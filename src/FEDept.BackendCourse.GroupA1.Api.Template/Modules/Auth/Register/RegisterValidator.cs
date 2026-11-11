using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;

public static class RegisterValidator
{
    public static IDictionary<string, string[]>? Validate(string? email, string? password)
    {
        var errors = new Dictionary<string, string[]>();

        var emailError = EmailValidator.Validate(email);
        var passwordError = PasswordValidator.Validate(password);

        if (emailError is not null)
        {
            errors["email"] = [emailError];
        }

        if (passwordError is not null)
        {
            errors["password"] =  [passwordError];
        }

        return errors.Count > 0 ? errors : null;
    }
}
