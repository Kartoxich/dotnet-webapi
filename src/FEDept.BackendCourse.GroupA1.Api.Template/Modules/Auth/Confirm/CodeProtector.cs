using System.Security.Cryptography;
using System.Text;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public static class CodeProtector
{
    public static byte[] Protect(string code, byte[] key) =>
        HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(code));

    public static bool Matches(string code, byte[] key, byte[] storedHash) =>
        CryptographicOperations.FixedTimeEquals(
            HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(code))
        , storedHash);
}
