namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;

public enum ChallengeMethod
{
    Otp,
    MagicLink
}

public static class ChallengeMethodExtensions
{
    public static string ToColumn(this ChallengeMethod method) => method switch
    {
        ChallengeMethod.Otp => "otp",
        ChallengeMethod.MagicLink => "magic_link",
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
    };
}
