using System.Security.Cryptography;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Domain;

public static class ConfirmationCode
{
    public static (string Code, byte[] Hash) New(byte[] key)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        return (code, CodeProtector.Protect(code, key));
    }
}
