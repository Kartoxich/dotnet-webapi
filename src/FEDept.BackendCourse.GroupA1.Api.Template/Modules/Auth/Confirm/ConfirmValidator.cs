namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public static class ConfirmValidator
{
    public static IDictionary<string, string[]>? Validate(Guid? attemptId, string? code)
    {
        var errors = new Dictionary<string, string[]>();

        var attemptIdError = AttemptIdValidator.Validate(attemptId);
        var codeError = CodeValidator.Validate(code);

        if (attemptIdError is not null)
        {
            errors["attemptId"] = [attemptIdError];
        }

        if (codeError is not null)
        {
            errors["code"] =  [codeError];
        }

        return errors.Count > 0 ? errors : null;
    }
}
