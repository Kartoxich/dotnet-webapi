using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register.Email;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public class ConfirmValidator
{
    public static IDictionary<string, string[]>? Validate(string? email, string? code)
    {
        var errors = new Dictionary<string, string[]>();

        var emailError = EmailValidator.Validate(email);
        var codeError = CodeValidator.Validate(code);

        if (emailError is not null)
        {
            errors["email"] = [emailError];
        }

        if (codeError is not null)
        {
            errors["code"] =  [codeError];
        }

        return errors.Count > 0 ? errors : null;
    }
}
